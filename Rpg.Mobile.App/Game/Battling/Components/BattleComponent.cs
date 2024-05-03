using Rpg.Mobile.App.Game.Battling.Gamemaster;
using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk;
using static Rpg.Mobile.App.Game.Sprites;
using Math = System.Math;

namespace Rpg.Mobile.App.Game.Battling.Components;

public class BattleComponent : ComponentBase
{
    private static int TileSize => MapComponent.TileSize;

    private readonly MapComponent _map;
    private readonly TileShadowComponent _moveShadow;
    private readonly TileShadowComponent _attackShadow;
    private readonly TileShadowComponent _currentUnitShadow;

    private readonly PathCalculator _path = new();
    private readonly DamageCalculator _damage = new(Rng.Instance);
    private readonly SpellDamageCalculator _spellDamage = new(Rng.Instance);

    private Dictionary<BattleUnitState, BattleUnitComponent> _unitComponents = new();
    private ITween<PointF>? _unitTween;

    private BattleUnitComponent? CurrentUnit => _state.CurrentUnit is not null ? _unitComponents[_state.CurrentUnit] : null;

    // TODO: remove
    private readonly MenuComponent _battleMenu;
    private readonly BattleState _state;
    private readonly BattleStateService _battleService;

    public BattleComponent(
        // TODO: remove extra components
         MenuComponent battleMenu, PointF location, BattleState battle, BattleStateService battleService) 
        : base(CalcBounds(location, battle.Map.Width, battle.Map.Height, TileSize))
    {
        _battleMenu = battleMenu;
        _battleService = battleService;

        _state = battle;
        AddChild(_map = new(battle.Map));
        AddChild(_moveShadow = new(_map.Bounds) { BackColor = Colors.BlueViolet.WithAlpha(.3f) });
        AddChild(_attackShadow = new(_map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) });
        AddChild(_currentUnitShadow = new(_map.Bounds) { BackColor = Colors.WhiteSmoke.WithAlpha(.5f) });

        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<ActiveUnitChangedEvent>(UnitChanged);
        Bus.Global.Subscribe<BattleStepChangedEvent>(BattleStepChanged);
        Bus.Global.Subscribe<BattleStartedEvent>(_ => StartBattle());
    }

    public override void Update(float deltaTime)
    {
        if (_unitTween is not null)
            CurrentUnit.Position = _unitTween.Advance(deltaTime);

        var walkShadows = _state.WalkableTiles.Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize));
        _moveShadow.Shadows.Set(walkShadows);

        var currentUnitPosition = _state.UnitCoordinates[CurrentUnit.State];
        _currentUnitShadow.Shadows.SetSingle(new(currentUnitPosition.X * TileSize, currentUnitPosition.Y * TileSize, TileSize, TileSize));

        var gridToUnit = _unitComponents.Values.ToLookup(x => GetTileForPosition(x.Position));
        if (_state.Step == BattleStep.SelectingAttackTarget)
        {
            var currentUnit = CurrentUnit;
            var currentUnitTile = GetTileForPosition(CurrentUnit.Position);
            var shadows = _path
                .CreateFanOutArea(
                    currentUnitTile,
                    new(_map.State.Width, _map.State.Height),
                    CurrentUnit.State.Stats.AttackMinRange,
                    CurrentUnit.State.Stats.AttackMaxRange)
                .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.State.PlayerId != currentUnit.State.PlayerId))
                .Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize))
                .ToList();

            _attackShadow.Shadows.AddRange(shadows);
        }

        if (_state.CurrentSpell is not null)
        {
            var currentUnitTile = GetTileForPosition(CurrentUnit.Position);
            var shadows = _path
                .CreateFanOutArea(
                    new(currentUnitTile.X, currentUnitTile.Y),
                    new(_map.State.Width, _map.State.Height),
                    _state.CurrentSpell.MinRange,
                    _state.CurrentSpell.MaxRange)
                .Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize))
                .ToList();

            _attackShadow.Shadows.AddRange(shadows);
        }
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
    }

    private void StartBattle()
    {
        _unitComponents = _state.TurnOrder
            .Select(CreateBattleUnitComponent)
            .Select(AddChild)
            .ToDictionary(x => x.State);

        foreach (var component in _unitComponents.Values)
        {
            var point = _state.UnitCoordinates[component.State];
            component.Position = GetPositionForTile(point, component.Bounds.Size);
        }
    }

    private static BattleUnitComponent CreateBattleUnitComponent(BattleUnitState state) =>
        (state.Stats.UnitType, state.PlayerId) switch
        {
            (BattleUnitType.Archer, 0) => new BattleUnitComponent(Images.ArcherIdle01, state),
            (BattleUnitType.Healer, 0) => new BattleUnitComponent(Images.HealerIdle01, state),
            (BattleUnitType.Mage, 0) => new BattleUnitComponent(Images.MageIdle01, state),
            (BattleUnitType.Warrior, 0) => new BattleUnitComponent(Images.WarriorIdle01, state),
            (BattleUnitType.Ninja, 0) => new BattleUnitComponent(Images.NinjaIdle01, state),
            (BattleUnitType.Archer, 1) => new BattleUnitComponent(Images.ArcherIdle02, state),
            (BattleUnitType.Healer, 1) => new BattleUnitComponent(Images.HealerIdle02, state),
            (BattleUnitType.Mage, 1) => new BattleUnitComponent(Images.MageIdle02, state),
            (BattleUnitType.Warrior, 1) => new BattleUnitComponent(Images.WarriorIdle02, state),
            (BattleUnitType.Ninja, 1) => new BattleUnitComponent(Images.NinjaIdle02, state),
            _ => throw new ArgumentException()
        };

    private Point GetTileForPosition(PointF point) => new((int)(point.X / TileSize), (int)(point.Y / TileSize));

    private PointF GetPositionForTile(Point point, SizeF componentSize) => GetPositionForTile(point.X, point.Y, componentSize);

    private PointF GetPositionForTile(int x, int y, SizeF componentSize)
    {
        var marginX = (TileSize - componentSize.Width) / 2;
        var marginY = (TileSize - componentSize.Height) / 2;
        return new((x * TileSize) + marginX, (y * TileSize) + marginY);
    }

    private static RectF CalcBounds(PointF position, int width, int height, float size) =>
        new(position.X, position.Y, width * size, height * size);

    private void TileClicked(TileClickedEvent evnt)
    {
        var currentUnit = CurrentUnit;
        var clickedTileCenter = GetPositionForTile(evnt.Tile, SizeF.Zero);

        var enemies = _unitComponents.Values
            .Where(a => a.State.PlayerId != currentUnit.State.PlayerId && a.Visible)
            .ToLookup(a => GetTileForPosition(a.Position));
        
        if (_state.Step == BattleStep.SelectingAttackTarget &&
            _attackShadow.Shadows.Any(a => a.Contains(clickedTileCenter) && enemies.Contains(evnt.Tile)))
        {
            var enemy = enemies[evnt.Tile].First();
            var damage = _damage.CalcDamage(currentUnit.State.Stats.Attack, currentUnit.State.Stats.Defense);
            DamageUnit(enemy, damage);
            _battleService.AdvanceToNextUnit();
        }

        if (_state.Step == BattleStep.CastingSpell && _state.CurrentSpell is not null)
        {
            CastSpell(_state.CurrentSpell, evnt.Tile);
        }

        if (_moveShadow.Shadows.Any(a => a.Contains(clickedTileCenter)))
        {
            _state.UnitCoordinates[CurrentUnit.State] = evnt.Tile;

            var finalTarget = GetPositionForTile(evnt.Tile, CurrentUnit.Bounds.Size);
            _unitTween = CurrentUnit.Position.TweenTo(500f, finalTarget);
        }
    }

    private void TileHovered(TileHoveredEvent evnt)
    {
        var hoveredUnit = _state.UnitCoordinates.ContainsValue(evnt.Tile)
            ? _state.UnitCoordinates.First(x => x.Value == evnt.Tile).Key
            : null;

        Bus.Global.Publish(new BattleTileHoveredEvent(hoveredUnit));
    }

    private void UnitChanged(ActiveUnitChangedEvent evnt)
    {
        _unitTween = null;
    }

    private void BattleStepChanged(BattleStepChangedEvent evnt)
    {
        if (_state.Step == BattleStep.Moving)
        {
            _battleMenu.SetButtons(
                new("Attack", _ => _battleService.ChangeBattleState(BattleStep.SelectingAttackTarget)),
                new("Magic", _ => _battleService.ChangeBattleState(BattleStep.CastingSpell)),
                new("Wait", _ => _battleService.AdvanceToNextUnit()));
        }

        if (_state.Step == BattleStep.SelectingAttackTarget)
        {
            _battleMenu.SetButtons(new ButtonState("Back", _ => _battleService.ChangeBattleState(BattleStep.Moving)));
        }

        if (_state.Step == BattleStep.CastingSpell)
        {
            _battleMenu.SetButtons(
                CurrentUnit.State.Spells
                    .Select(x => new ButtonState(x.Name, _ => _state.CurrentSpell = x))
                    .Append(new("Back", _ => _battleService.ChangeBattleState(BattleStep.Moving)))
                    .ToArray());
        }
    }

    private void CastSpell(SpellState spell, Point position)
    {
        var currentUnit = CurrentUnit;
        if (currentUnit.State.RemainingMp < spell.MpCost)
        {
            //_stats.Label = "Not enough MP"; // TODO: Less hacky way.
            return;
        }

        var targets = _unitComponents.Values
            .Where(x =>
                GetTileForPosition(x.Position) == position &&
                (spell.TargetsEnemies && x.State.PlayerId != currentUnit.State.PlayerId ||
                 spell.TargetsFriendlies && x.State.PlayerId == currentUnit.State.PlayerId))
            .ToList();

        if (targets.Count == 0)
            return;

        currentUnit.State.RemainingMp -= spell.MpCost;
        var damage = _spellDamage.CalcDamage(spell);
        foreach (var target in targets)
        {
            DamageUnit(target, damage);
        }

        _battleService.AdvanceToNextUnit();
    }

    private void DamageUnit(BattleUnitComponent enemy, int damage)
    {
        enemy.State.RemainingHealth = damage >= 0
            ? Math.Max(enemy.State.RemainingHealth - damage, 0)
            : Math.Min(enemy.State.Stats.MaxHealth, enemy.State.RemainingHealth - damage);

        if (enemy.State.RemainingHealth <= 0)
        {
            enemy.Visible = false;
            _unitComponents.Remove(enemy.State);
            _state.UnitCoordinates.Remove(enemy.State);
        }
    }
}


public record BattleTileHoveredEvent(BattleUnitState? Unit) : IEvent;
