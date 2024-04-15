using Rpg.Mobile.App.Game.Battling.Backend;
using Rpg.Mobile.App.Game.Battling.Components.Menus;
using Rpg.Mobile.App.Infrastructure;
using Rpg.Mobile.GameSdk;
using static Rpg.Mobile.App.Game.Sprites;
using Math = System.Math;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Game.Battling.Components;

public enum BattleStep
{
    Starting,
    Moving,
    CastingSpell,
    SelectingAttackTarget
}

public class BattleState
{
    public MapState Map { get; set; }
    public List<BattleUnitState> BattleUnits { get; set; } = new();

    public Point CurrentPosition { get; set; } = Point.Empty;
    public Dictionary<Point, BattleUnitState> StartingPositions { get; set; } = new();

    public BattleUnitState? ActiveUnit { get; set; }
    public StepState CurrentStep { get; set; } = new();

    public BattleState(MapState map)
    {
        Map = map;
    }

    public class StepState
    {
        public BattleStep Current { get; set; } = BattleStep.Starting;
        public List<Point> MovementPath { get; set; } = new();
        public SpellState? TargetingSpell { get; set; }
    }
}

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

    private List<BattleUnitComponent> _battleUnits;
    private ITween<PointF>? _unitTween;
    private Point _gridStart;
    private BattleStep _step = BattleStep.Moving;
    private SpellState? _currentSpell;

    private int _currentUnitIndex = -1;
    private BattleUnitComponent CurrentUnit => _battleUnits[_currentUnitIndex];

    // TODO: remove
    private readonly StatSheetComponent _stats;
    private readonly MenuComponent _battleMenu;

    public BattleComponent(
        // TODO: remove extra components
        StatSheetComponent stats, MenuComponent battleMenu, PointF location, MapState battle) 
        : base(CalcBounds(location, battle.ColumnCount, battle.RowCount, TileSize))
    {
        _stats = stats;
        _battleMenu = battleMenu;

        AddChild(_map = new(battle));
        AddChild(_moveShadow = new(_map.Bounds) { BackColor = Colors.BlueViolet.WithAlpha(.3f) });
        AddChild(_attackShadow = new(_map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) });
        AddChild(_currentUnitShadow = new(_map.Bounds) { BackColor = Colors.WhiteSmoke.WithAlpha(.5f) });

        _battleUnits = _map.State.TurnOrder
            .Select(CreateBattleUnitComponent)
            .Select(AddChild)
            .ToList();

        foreach (var component in _battleUnits)
        {
            var point = _map.State.UnitTiles[component.State];
            component.Position = GetPositionForTile(point, component.Bounds.Size);
        }

        Bus.Subscribe<TileClickedEvent>(TileClicked);
    }

    public override void Update(float deltaTime)
    {
        if (_unitTween is not null)
            CurrentUnit.Position = _unitTween.Advance(deltaTime);

        _moveShadow.Shadows.Clear();
        _attackShadow.Shadows.Clear();

        var currentUnitPosition = _map.State.UnitTiles[CurrentUnit.State];
        _currentUnitShadow.Shadows.Set(new(currentUnitPosition.X * TileSize, currentUnitPosition.Y * TileSize, TileSize, TileSize));

        var gridToUnit = _battleUnits.ToLookup(x => GetTileForPosition(x.Position));
        if (_step == BattleStep.Moving)
        {
            var shadows = _path
                .CreateFanOutArea(_gridStart, new(_map.State.ColumnCount, _map.State.RowCount), CurrentUnit.State.Stats.Movement)
                .Where(x => !_map.State.UnitTiles.ContainsValue(new(x.X, x.Y)) && _map.State.Tiles[x.X, x.Y].Type != TerrainType.Rock)
                .Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize))
                .ToList();

            _moveShadow.Shadows.AddRange(shadows);
        }

        if (_step == BattleStep.SelectingAttackTarget)
        {
            var currentUnit = CurrentUnit;
            var shadows = _path
                .CreateFanOutArea(
                    GetTileForPosition(CurrentUnit.Position),
                    new(_map.State.ColumnCount, _map.State.RowCount),
                    CurrentUnit.State.Stats.AttackMinRange,
                    CurrentUnit.State.Stats.AttackMaxRange)
                .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.State.PlayerId != currentUnit.State.PlayerId))
                .Select(x => new RectF(x.X * TileSize, x.Y * TileSize, TileSize, TileSize))
                .ToList();

            _attackShadow.Shadows.AddRange(shadows);
        }

        if (_currentSpell is not null)
        {
            var shadows = _path
                .CreateFanOutArea(
                    GetTileForPosition(CurrentUnit.Position),
                    new(_map.State.ColumnCount, _map.State.RowCount),
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
        if (_currentUnitIndex >= 0)
            throw new NotSupportedException("Battle already started.");

        AdvanceToNextUnit();
    }

    public void AdvanceToNextUnit()
    {
        _currentUnitIndex = _currentUnitIndex + 1 < _battleUnits.Count ? _currentUnitIndex + 1 : 0;
        if (_currentUnitIndex == 0)
            _battleUnits = _battleUnits.Shuffle(Rng.Instance).ToList();

        _gridStart = GetTileForPosition(CurrentUnit.Position);
        _unitTween = null;

        Bus.Publish(new ActiveUnitChanged(CurrentUnit.State));

        _step = BattleStep.Moving;
        Bus.Publish(new BattleStepChanged(_step));

        UpdateMenuState(_step);
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

    private static RectF CalcBounds(PointF position, int colCount, int rowCount, float size) =>
        new(position.X, position.Y, colCount * size, rowCount * size);

    private void TileClicked(TileClickedEvent evnt)
    {
        var x = evnt.Tile.X;
        var y = evnt.Tile.Y;

        var currentUnit = CurrentUnit;
        var clickedTileCenter = GetPositionForTile(x, y, SizeF.Zero);
        var units = _battleUnits
            .Where(a => a.Visible)
            .ToLookup(a => GetTileForPosition(a.Position));

        var enemies = _battleUnits
            .Where(a => a.State.PlayerId != currentUnit.State.PlayerId && a.Visible)
            .ToLookup(a => GetTileForPosition(a.Position));

        var gridPosition = new Point(x, y);
        if (_step == BattleStep.SelectingAttackTarget &&
            _attackShadow.Shadows.Any(a => a.Contains(clickedTileCenter) && enemies.Contains(gridPosition)))
        {
            var enemy = enemies[gridPosition].First();
            var damage = _damage.CalcDamage(currentUnit.State.Stats.Attack, currentUnit.State.Stats.Defense);
            DamageUnit(enemy, damage);
            AdvanceToNextUnit();
        }

        if (_step == BattleStep.CastingSpell && _currentSpell is not null)
        {
            CastSpell(_currentSpell, gridPosition);
        }

        if (_moveShadow.Shadows.Any(a => a.Contains(clickedTileCenter)))
        {
            _map.State.UnitTiles[CurrentUnit.State] = new(x, y);

            var finalTarget = GetPositionForTile(x, y, CurrentUnit.Bounds.Size);
            _unitTween = CurrentUnit.Position.TweenTo(500f, finalTarget);
        }

        if (_step == BattleStep.Moving && units.Contains(gridPosition))
        {
            _stats.ChangeUnit(units[gridPosition].First());
        }
    }

    private void CastSpell(SpellState spell, Point position)
    {
        var currentUnit = CurrentUnit;
        if (currentUnit.State.RemainingMp < spell.MpCost)
        {
            _stats.Label = "Not enough MP"; // TODO: Less hacky way.
            return;
        }

        var targets = _battleUnits
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
            _battleUnits.Remove(enemy);
            _map.State.UnitTiles.Remove(enemy.State);
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

public record ActiveUnitChanged(BattleUnitState State) : IEvent;
public record BattleStepChanged(BattleStep Step) : IEvent;
