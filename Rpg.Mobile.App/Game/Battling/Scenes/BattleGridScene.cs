using Rpg.Mobile.App.Game.Battling.Components;
using Rpg.Mobile.App.Game.Battling.Components.Menus;
using Rpg.Mobile.App.Game.Battling.Domain;
using Rpg.Mobile.App.Game.Battling.Domain.Battles;
using Rpg.Mobile.App.Game.Menu;
using Rpg.Mobile.App.Windows;
using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;
using static Rpg.Mobile.App.Game.Sprites;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Game.Battling.Scenes;

public enum BattleMenuState
{
    SelectingAction,
    SelectingMagic,
    SelectingAttack
}

public class BattleGridScene : SceneBase
{
    private readonly MapComponent _map;
    private readonly GridComponent _grid;
    private readonly MiniMapComponent _miniMap;
    private readonly MenuComponent _battleMenu;
    private readonly TileShadowComponent _moveShadow;
    private readonly TileShadowComponent _attackShadow;
    private readonly TileShadowComponent _currentUnitShadow;
    private readonly StatSheetComponent _stats;
    private readonly MenuComponent _stateMenu;
    private readonly TextboxComponent _mouseComponent;
    private readonly TextboxComponent _hoverComponent;

    private List<BattleUnitComponent> _battleUnits;
    private ITween<PointF>? _unitTween;
    private ITween<PointF>? _cameraTween;
    private int _currentUnitIndex = -1;
    private Point _gridStart;

    private readonly DamageCalculator _damage = new(Rng.Instance);
    private readonly SpellDamageCalculator _spellDamage = new(Rng.Instance);
    private readonly PathCalculator _path = new();
    private BattleMenuState _menuState = BattleMenuState.SelectingAction;
    private SpellState? _currentSpell;
    private readonly IMouse _mouse;
    private readonly VisualElement _element;

    public BattleUnitComponent CurrentUnit => _battleUnits[_currentUnitIndex];

    // Todo: remove element stuff and abstract into game engine.
    public BattleGridScene(IMouse mouse, VisualElement element)
    {
        _mouse = mouse;
        _element = element;

        Add(_battleMenu = new(new(900f, 100f, 150f, 200f)));
        Add(_miniMap = new(MiniMapClicked, new(_battleMenu.Bounds.Right + 100f, _battleMenu.Bounds.Bottom + 100f, 200f, 200f)) { IgnoreCamera = true });
        Add(_map = new(Battles.Demo));
        Add(_mouseComponent = new(new(_miniMap.AbsoluteBounds.Left, _miniMap.AbsoluteBounds.Bottom, 300f, 100f), "") { IgnoreCamera = true });
        Add(_stats = new(new(900f, _battleMenu.Bounds.Bottom + 30f, 150, 300f)) { IgnoreCamera = true });
        Add(_hoverComponent = new(new(_stats.Bounds.Left, _stats.Bounds.Bottom + 100f, 300f, 200f), "")
        {
            IgnoreCamera = true,
            BackColor = Colors.DeepSkyBlue
        });

        _stateMenu = new(new(1200f, _battleMenu.Bounds.Bottom + 5f, _battleMenu.Bounds.Width, _battleMenu.Bounds.Height));
        _stateMenu.SetButtons(new("Save State", SaveStateClicked), new("Load State", LoadStateClicked));

        _map.AddChild(_moveShadow = new(_map.Bounds) { BackColor = Colors.BlueViolet.WithAlpha(.3f) });
        _map.AddChild(_attackShadow = new(_map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) });
        _map.AddChild(_currentUnitShadow = new(_map.Bounds) { BackColor = Colors.WhiteSmoke.WithAlpha(.5f) });
        _map.AddChild(_grid = new(10, 12));

        Bus.Subscribe<TileHoveredEvent>(x => _hoverComponent.Label = $"{x.Tile.X}x{x.Tile.Y}");
        Bus.Subscribe<TileClickedEvent>(TileClicked);

        _battleUnits = _map.State.TurnOrder
            .Select(CreateBattleUnitComponent)
            .Select(_map.AddChild)
            .ToList();

        foreach (var component in _battleUnits)
        {
            var point = _map.State.UnitTiles[component.State];
            component.Position = _grid.GetPositionForTile(point, component.Bounds.Size);
        }

        ActiveCamera.Offset = new PointF(220f, 100f);
        AdvanceToNextUnit();
    }

    public override void Update(float deltaTime)
    {
        if (_unitTween is not null)
            CurrentUnit.Position = _unitTween.Advance(deltaTime);

        if (_cameraTween is not null)
            ActiveCamera.Offset = _cameraTween.Advance(deltaTime);

        _moveShadow.Shadows.Clear();
        _attackShadow.Shadows.Clear();
        _currentUnitShadow.Shadows.Clear();

        var screenCursor = _mouse.GetScreenMousePosition();
        var windowCursor = _mouse.GetRelativeClientPosition();
        _mouseComponent.Label = new[]
        {
            $"Screen: {screenCursor.X},{screenCursor.Y}",
            $"Client: {windowCursor.X},{windowCursor.Y}",
        }.JoinLines(true);

        var currentUnitPosition = _map.State.UnitTiles[CurrentUnit.State];
        _currentUnitShadow.Shadows.Add(new(currentUnitPosition.X * _grid.Size, currentUnitPosition.Y * _grid.Size, _grid.Size, _grid.Size));

        var gridToUnit = _battleUnits.ToLookup(x => _grid.GetTileForPosition(x.Position));
        if (_menuState == BattleMenuState.SelectingAction)
        {
            var shadows = _path
                .CreateFanOutArea(_gridStart, new(_grid.ColCount, _grid.RowCount), CurrentUnit.State.Stats.Movement)
                .Where(x => !_map.State.UnitTiles.ContainsValue(new(x.X, x.Y)) && _map.State.Tiles[x.X, x.Y].Type != TerrainType.Rock)
                .Select(x => new RectF(x.X * _grid.Size, x.Y * _grid.Size, _grid.Size, _grid.Size))
                .ToList();

            _moveShadow.Shadows.AddRange(shadows);
        }

        if (_menuState == BattleMenuState.SelectingAttack)
        {
            var currentUnit = CurrentUnit;
            var shadows = _path
                .CreateFanOutArea(
                    _grid.GetTileForPosition(CurrentUnit.Position),
                    new(_grid.ColCount, _grid.RowCount),
                    CurrentUnit.State.Stats.AttackMinRange,
                    CurrentUnit.State.Stats.AttackMaxRange)
                .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.State.PlayerId != currentUnit.State.PlayerId))
                .Select(x => new RectF(x.X * _grid.Size, x.Y * _grid.Size, _grid.Size, _grid.Size))
                .ToList();

            _attackShadow.Shadows.AddRange(shadows);
        }

        if (_currentSpell is not null)
        {
            var shadows = _path
                .CreateFanOutArea(
                    _grid.GetTileForPosition(CurrentUnit.Position),
                    new(_grid.ColCount, _grid.RowCount),
                    _currentSpell.MinRange,
                    _currentSpell.MaxRange)
                .Select(x => new RectF(x.X * _grid.Size, x.Y * _grid.Size, _grid.Size, _grid.Size))
                .ToList();

            _attackShadow.Shadows.AddRange(shadows);
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

    private void MiniMapClicked(PointF touch)
    {
        var xPercent = touch.X / _miniMap.Bounds.Width;
        var yPercent = touch.Y / _miniMap.Bounds.Height;
        var target = new PointF(ActiveCamera.Size.Width * xPercent * 2, ActiveCamera.Size.Height * yPercent * 2);
        _cameraTween = ActiveCamera.Offset.TweenTo(target, 1000f);
    }

    private void SaveStateClicked(IEnumerable<PointF> touches)
    {
    }

    private void LoadStateClicked(IEnumerable<PointF> touches)
    {
    }

    private void TileClicked(TileClickedEvent evnt)
    {
        var x = evnt.Tile.X;
        var y = evnt.Tile.Y;

        var currentUnit = CurrentUnit;
        var clickedTileCenter = _grid.GetPositionForTile(x, y, SizeF.Zero);
        var units = _battleUnits
            .Where(a => a.Visible)
            .ToLookup(a => _grid.GetTileForPosition(a.Position));

        var enemies = _battleUnits
            .Where(a => a.State.PlayerId != currentUnit.State.PlayerId && a.Visible)
            .ToLookup(a => _grid.GetTileForPosition(a.Position));

        var gridPosition = new Point(x, y);
        if (_menuState == BattleMenuState.SelectingAttack &&
            _attackShadow.Shadows.Any(a => a.Contains(clickedTileCenter) && enemies.Contains(gridPosition)))
        {
            var enemy = enemies[gridPosition].First();
            var damage = _damage.CalcDamage(currentUnit.State.Stats.Attack, currentUnit.State.Stats.Defense);
            DamageUnit(enemy, damage);
            AdvanceToNextUnit();
        }

        if (_menuState == BattleMenuState.SelectingMagic && _currentSpell is not null)
        {
            CastSpell(_currentSpell, gridPosition);
        }

        if (_moveShadow.Shadows.Any(a => a.Contains(clickedTileCenter)))
        {
            _map.State.UnitTiles[CurrentUnit.State] = new(x, y);
            
            var finalTarget = _grid.GetPositionForTile(x, y, CurrentUnit.Bounds.Size);
            _unitTween = CurrentUnit.Position.TweenTo(500f, finalTarget);
        }

        if (_menuState == BattleMenuState.SelectingAction && units.Contains(gridPosition))
        {
            _stats.ChangeUnit(units[gridPosition].First());
        }
    }

    private void AdvanceToNextUnit()
    {
        _currentUnitIndex = _currentUnitIndex + 1 < _battleUnits.Count ? _currentUnitIndex + 1 : 0;
        if (_currentUnitIndex == 0)
            _battleUnits = _battleUnits.Shuffle(Rng.Instance).ToList();

        _gridStart = _grid.GetTileForPosition(CurrentUnit.Position);
        UpdateMenuState(BattleMenuState.SelectingAction);
        _stats.ChangeUnit(CurrentUnit);
        _unitTween = null;
    }

    private void UpdateMenuState(BattleMenuState menuState)
    {
        _menuState = menuState;
        _currentSpell = null;

        if (_menuState == BattleMenuState.SelectingAction)
        {
            _battleMenu.SetButtons(
                new("Attack", _ => UpdateMenuState(BattleMenuState.SelectingAttack)),
                new("Magic", _ => UpdateMenuState(BattleMenuState.SelectingMagic)),
                new("Wait", _ => AdvanceToNextUnit()));
        }

        if (_menuState == BattleMenuState.SelectingAttack)
        {
            _battleMenu.SetButtons(new ButtonState("Back", _ => UpdateMenuState(BattleMenuState.SelectingAction)));
        }

        if (_menuState == BattleMenuState.SelectingMagic)
        {
            _battleMenu.SetButtons(
                CurrentUnit.State.Spells
                    .Select(x => new ButtonState(x.Name, _ => _currentSpell = x))
                    .Append(new("Back", _ => UpdateMenuState(BattleMenuState.SelectingAction)))
                    .ToArray());
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
                _grid.GetTileForPosition(x.Position) == position &&
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
}
