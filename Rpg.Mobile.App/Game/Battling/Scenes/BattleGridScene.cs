using Rpg.Mobile.App.Game.Battling.Calculators;
using Rpg.Mobile.App.Game.Battling.Components;
using Rpg.Mobile.App.Game.Menu;
using Rpg.Mobile.GameSdk;
using static Rpg.Mobile.App.Game.Sprites;
using Point = System.Drawing.Point;

namespace Rpg.Mobile.App.Game.Battling.Scenes;

public enum BattleGridState
{
    SelectingAction,
    SelectingAttackTarget
}

public class BattleGridScene : SceneBase
{
    private readonly MapComponent _map;
    private readonly GridComponent _grid;
    private readonly MiniMapComponent _miniMap;
    private readonly ButtonComponent _attackButton;
    private readonly ButtonComponent _waitButton;
    private readonly TileShadowComponent _moveShadow;
    private readonly TileShadowComponent _attackShadow;
    private readonly StatSheetComponent _stats;

    private List<BattleUnitComponent> _battleUnits = new();
    private ITween<PointF>? _unitTween;
    private ITween<PointF>? _cameraTween;
    private int _currentUnitIndex = -1;
    private Point _gridStart;

    private readonly DamageCalculator _damage = new(new Rng(new()));
    private readonly PathCalculator _path = new();

    private BattleGridState _currentState = BattleGridState.SelectingAction;

    public BattleUnitComponent CurrentUnit => _battleUnits[_currentUnitIndex];

    public BattleGridScene()
    {
        _grid = new(GridClicked, 10, 12);
        _map = Add(new MapComponent(new(0f, 0f, _grid.Bounds.Width, _grid.Bounds.Height)));
        _moveShadow = new(_map.Bounds) { BackColor = Colors.SlateGray.WithAlpha(.7f) };
        _attackShadow = new(_map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) };
        
        var buttonTop = 0f;
        _attackButton = Add(new ButtonComponent(new(1200f, buttonTop += 50f, 100f, 50f), "Attack", OnAttack) { IgnoreCamera = true });
        _waitButton = Add(new ButtonComponent(new(1200f, buttonTop += 60f, 100f, 50f), "Wait", OnWait) { IgnoreCamera = true });

        _miniMap = Add(new MiniMapComponent(MiniMapClicked, new(1100f, _waitButton.Bounds.Bottom + 100f, 200f, 200f))
        {
            IgnoreCamera = true
        });

        _map.AddChild(_moveShadow);
        _map.AddChild(_attackShadow);
        _map.AddChild(_grid);

        var units = new[]
        {
            _map.AddChild(new BattleUnitComponent(0, Images.ArcherIdle01, StatPresets.Archer)),
            _map.AddChild(new BattleUnitComponent(0, Images.HealerIdle01, StatPresets.Healer)),
            _map.AddChild(new BattleUnitComponent(0, Images.MageIdle01, StatPresets.Mage)),
            _map.AddChild(new BattleUnitComponent(0, Images.NinjaIdle01, StatPresets.Ninja)),
            _map.AddChild(new BattleUnitComponent(0, Images.WarriorIdle01, StatPresets.Warrior)),
            _map.AddChild(new BattleUnitComponent(1, Images.ArcherIdle02, StatPresets.Archer)),
            _map.AddChild(new BattleUnitComponent(1, Images.HealerIdle02, StatPresets.Healer)),
            _map.AddChild(new BattleUnitComponent(1, Images.MageIdle02, StatPresets.Mage)),
            _map.AddChild(new BattleUnitComponent(1, Images.NinjaIdle02, StatPresets.Ninja)),
            _map.AddChild(new BattleUnitComponent(1, Images.WarriorIdle02, StatPresets.Warrior))
        };

        _stats = Add(new StatSheetComponent(new(1100f, _miniMap.Bounds.Bottom + 100f, 200f, 300f)) { IgnoreCamera = true });
        InitializeBattlefield(units);
    }

    private void InitializeBattlefield(BattleUnitComponent[] units)
    {
        // temporary solution
        _battleUnits = units.OrderBy(_ => Guid.NewGuid()).ToList();

        foreach (var (unit, index) in _battleUnits.Where(x => x.PlayerId == 0).Select((x, i) => (x, i)))
        {
            unit.Position = _grid.GetPositionForTile(1, (index * 2) + 1, unit.Bounds.Size);
        }

        foreach (var (unit, index) in _battleUnits.Where(x => x.PlayerId == 1).Select((x, i) => (x, i)))
        {
            unit.Position = _grid.GetPositionForTile(8, (index * 2) + 1, unit.Bounds.Size);
        }

        ActiveCamera.Offset = new PointF(220f, 100f);
        AdvanceToNextUnit();
    }

    public void OnAttack(IEnumerable<PointF> touches)
    {
        _currentState = _currentState == BattleGridState.SelectingAction
            ? BattleGridState.SelectingAttackTarget
            : BattleGridState.SelectingAction;
    }

    public void OnWait(IEnumerable<PointF> touches) => AdvanceToNextUnit();

    public void MiniMapClicked(PointF touch)
    {
        var xPercent = touch.X / _miniMap.Bounds.Width;
        var yPercent = touch.Y / _miniMap.Bounds.Height;
        var target = new PointF(ActiveCamera.Size.Width * xPercent, ActiveCamera.Size.Height * yPercent);
        _cameraTween = ActiveCamera.Offset.TweenTo(target, 400f);
    }

    public void GridClicked(int x, int y)
    {
        var currentUnit = CurrentUnit;
        var clickedTileCenter = _grid.GetPositionForTile(x, y, SizeF.Zero);
        var units = _battleUnits
            .Where(a => a.IsVisible)
            .ToLookup(a => _grid.GetTileForPosition(a.Position));

        var enemies = _battleUnits
            .Where(a => a.PlayerId != currentUnit.PlayerId && a.IsVisible)
            .ToLookup(a => _grid.GetTileForPosition(a.Position));

        var gridPosition = new Point(x, y);
        if (_attackShadow.Shadows.Any(a => a.Contains(clickedTileCenter) && enemies.Contains(gridPosition)))
        {
            var enemy = enemies[gridPosition].First();
            var damage = _damage.CalcDamage(currentUnit.State.Attack, currentUnit.State.Defense);
            enemy.State.RemainingHealth = Math.Max(enemy.State.RemainingHealth - damage, 0);
            if (enemy.State.RemainingHealth <= 0)
            {
                enemy.IsVisible = false;
                _battleUnits.Remove(enemy);
            }

            AdvanceToNextUnit();
        }

        if (_moveShadow.Shadows.Any(a => a.Contains(clickedTileCenter)))
        {
            var finalTarget = _grid.GetPositionForTile(x, y, CurrentUnit.Bounds.Size);
            _unitTween = CurrentUnit.Position.TweenTo(500f, finalTarget);
        }

        if (_currentState == BattleGridState.SelectingAction && units.Contains(gridPosition))
        {
            _stats.ChangeUnit(units[gridPosition].First());
        }
    }

    public override void Update(float deltaTime)
    {
        if (_unitTween is not null)
            CurrentUnit.Position = _unitTween.Advance(deltaTime);

        if (_cameraTween is not null)
            ActiveCamera.Offset = _cameraTween.Advance(deltaTime);

        _moveShadow.Shadows.Clear();
        _attackShadow.Shadows.Clear();

        var gridToUnit = _battleUnits.ToLookup(x => _grid.GetTileForPosition(x.Position));
        if (_currentState == BattleGridState.SelectingAction)
        {
            var shadows = _path
                .CreateFanOutArea(_gridStart, new(_grid.ColCount, _grid.RowCount), CurrentUnit.State.Movement)
                .Where(x => !gridToUnit.Contains(x))
                .Select(x => new RectF(x.X * _grid.Size, x.Y * _grid.Size, _grid.Size, _grid.Size))
                .ToList();

            _moveShadow.Shadows.AddRange(shadows);
        }

        _attackButton.Label = _currentState == BattleGridState.SelectingAttackTarget ? "Back" : "Attack";
        if (_currentState == BattleGridState.SelectingAttackTarget)
        {
            var currentUnit = CurrentUnit;
            var shadows = _path
                .CreateFanOutArea(
                    _grid.GetTileForPosition(CurrentUnit.Position),
                    new(_grid.ColCount, _grid.RowCount),
                    CurrentUnit.State.AttackMinRange,
                    CurrentUnit.State.AttackMaxRange)
                .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.PlayerId != currentUnit.PlayerId))
                .Select(x => new RectF(x.X * _grid.Size, x.Y * _grid.Size, _grid.Size, _grid.Size))
                .ToList();

            _attackShadow.Shadows.AddRange(shadows);
        }
    }

    private void AdvanceToNextUnit()
    {
        _currentUnitIndex = _currentUnitIndex + 1 < _battleUnits.Count ? _currentUnitIndex + 1 : 0;
        _gridStart = _grid.GetTileForPosition(CurrentUnit.Position);
        _currentState = BattleGridState.SelectingAction;
        _unitTween = null;
    }
}
