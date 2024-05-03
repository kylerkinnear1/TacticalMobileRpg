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
    private Point _gridStart = new(0, 0);
    private BattleStep _step = BattleStep.Moving;
    private SpellState? _currentSpell;

    private BattleUnitComponent? CurrentUnit => _state.CurrentUnit is not null ? _unitComponents[_state.CurrentUnit] : null;

    // TODO: remove
    private readonly MenuComponent _battleMenu;
    private readonly BattleState _state;

    public BattleComponent(
        // TODO: remove extra components
         MenuComponent battleMenu, PointF location, BattleState battle) 
        : base(CalcBounds(location, battle.Map.Width, battle.Map.Height, TileSize))
    {
        _battleMenu = battleMenu;

        _state = battle;
        AddChild(_map = new(battle.Map));
        AddChild(_moveShadow = new(_map.Bounds) { BackColor = Colors.BlueViolet.WithAlpha(.3f) });
        AddChild(_attackShadow = new(_map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) });
        AddChild(_currentUnitShadow = new(_map.Bounds) { BackColor = Colors.WhiteSmoke.WithAlpha(.5f) });

        Bus.Global.Subscribe<TileClickedEvent>(TileClicked);
        Bus.Global.Subscribe<TileHoveredEvent>(TileHovered);
        Bus.Global.Subscribe<ActiveUnitChangedEvent>(UnitChanged);
    }

    public override void Update(float deltaTime)
    {
        if (_unitTween is not null)
            CurrentUnit.Position = _unitTween.Advance(deltaTime);

        _moveShadow.Shadows.Clear();
        _attackShadow.Shadows.Clear();

        var currentUnitPosition = _state.UnitCoordinates[CurrentUnit.State];
        _currentUnitShadow.Shadows.Set(new(currentUnitPosition.X * TileSize, currentUnitPosition.Y * TileSize, TileSize, TileSize));

        var gridToUnit = _unitComponents.Values.ToLookup(x => GetTileForPosition(x.Position));
        if (_step == BattleStep.Moving)
        {
            var shadows = _path
                .CreateFanOutArea(_gridStart, new(_map.State.Width, _map.State.Height), CurrentUnit.State.Stats.Movement)
                .Where(x => !_state.UnitCoordinates.ContainsValue(x) && _map.State.Tiles[x.X, x.Y].Type != TerrainType.Rock)
                .Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize))
                .ToList();

            _moveShadow.Shadows.AddRange(shadows);
        }

        if (_step == BattleStep.SelectingAttackTarget)
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

        if (_currentSpell is not null)
        {
            var currentUnitTile = GetTileForPosition(CurrentUnit.Position);
            var shadows = _path
                .CreateFanOutArea(
                    new(currentUnitTile.X, currentUnitTile.Y),
                    new(_map.State.Width, _map.State.Height),
                    _currentSpell.MinRange,
                    _currentSpell.MaxRange)
                .Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize))
                .ToList();

            _attackShadow.Shadows.AddRange(shadows);
        }
    }

    public override void Render(ICanvas canvas, RectF dirtyRect)
    {
    }

    public void StartBattle()
    {
        if (_state.ActiveUnitIndex >= 0)
            throw new NotSupportedException("Battle already started.");

        var player1Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        var player2Units = StatPresets.All.Shuffle(Rng.Instance).ToList();
        player2Units.ForEach(x => x.PlayerId = 1);

        foreach (var (unit, point) in player1Units.Zip(_state.Map.Player1Origins))
            _state.UnitCoordinates[unit] = point;

        foreach (var (unit, point) in player2Units.Zip(_state.Map.Player2Origins))
            _state.UnitCoordinates[unit] = point;

        var allUnits = player1Units.Concat(player2Units).Shuffle(Rng.Instance).ToList();
        _state.TurnOrder.AddRange(allUnits);

        _unitComponents = _state.TurnOrder
            .Select(CreateBattleUnitComponent)
            .Select(AddChild)
            .ToDictionary(x => x.State);

        foreach (var component in _unitComponents.Values)
        {
            var point = _state.UnitCoordinates[component.State];
            component.Position = GetPositionForTile(point, component.Bounds.Size);
        }

        AdvanceToNextUnit();
    }

    public void AdvanceToNextUnit()
    {
        _state.ActiveUnitIndex = _state.ActiveUnitIndex + 1 < _unitComponents.Count ? _state.ActiveUnitIndex + 1 : 0;
        _step = BattleStep.Moving;

        Bus.Global.Publish(new ActiveUnitChangedEvent(CurrentUnit.State));
        Bus.Global.Publish(new BattleStepChangedEvent(_step));
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
        var units = _unitComponents.Values
            .Where(a => a.Visible)
            .ToLookup(a => GetTileForPosition(a.Position));

        var enemies = _unitComponents.Values
            .Where(a => a.State.PlayerId != currentUnit.State.PlayerId && a.Visible)
            .ToLookup(a => GetTileForPosition(a.Position));
        
        if (_step == BattleStep.SelectingAttackTarget &&
            _attackShadow.Shadows.Any(a => a.Contains(clickedTileCenter) && enemies.Contains(evnt.Tile)))
        {
            var enemy = enemies[evnt.Tile].First();
            var damage = _damage.CalcDamage(currentUnit.State.Stats.Attack, currentUnit.State.Stats.Defense);
            DamageUnit(enemy, damage);
            AdvanceToNextUnit();
        }

        if (_step == BattleStep.CastingSpell && _currentSpell is not null)
        {
            CastSpell(_currentSpell, evnt.Tile);
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
        var unit = _unitComponents[evnt.State];
        _gridStart = GetTileForPosition(unit.Position);
        _unitTween = null;
    }

    private void BattleStepChanged(BattleStepChangedEvent evnt)
    {
        UpdateMenuState(_step);
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

        AdvanceToNextUnit();
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

    private void UpdateMenuState(BattleStep step)
    {
        _step = step;
        _currentSpell = null;

        if (_step == BattleStep.Moving)
        {
            _battleMenu.SetButtons(
                new("Attack", _ => UpdateMenuState(BattleStep.SelectingAttackTarget)),
                new("Magic", _ => UpdateMenuState(BattleStep.CastingSpell)),
                new("Wait", _ => AdvanceToNextUnit()));
        }

        if (_step == BattleStep.SelectingAttackTarget)
        {
            _battleMenu.SetButtons(new ButtonState("Back", _ => UpdateMenuState(BattleStep.Moving)));
        }

        if (_step == BattleStep.CastingSpell)
        {
            _battleMenu.SetButtons(
                CurrentUnit.State.Spells
                    .Select(x => new ButtonState(x.Name, _ => _currentSpell = x))
                    .Append(new("Back", _ => UpdateMenuState(BattleStep.Moving)))
                    .ToArray());
        }
    }
}

public record ActiveUnitChangedEvent(BattleUnitState State) : IEvent;
public record BattleStepChangedEvent(BattleStep Step) : IEvent;
public record BattleTileHoveredEvent(BattleUnitState? Unit) : IEvent;
