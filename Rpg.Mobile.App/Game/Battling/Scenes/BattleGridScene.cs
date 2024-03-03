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

    private List<BattleUnitComponent> _battleUnits = new();
    private ITween<PointF>? _gridTween;
    private ITween<PointF>? _cameraTween;
    private int _currentUnitIndex = -1;
    private Point _gridStart;

    private readonly DamageCalculator _damage = new(new Rng(new()));
    private readonly PathCalculator _path = new();

    private BattleGridState _currentState = BattleGridState.SelectingAction;

    public BattleUnitComponent CurrentUnit => _battleUnits[_currentUnitIndex];

    public BattleGridScene()
    {
        _grid = new(GridClicked, 10, 15);
        _map = Add(new MapComponent(new(0f, 0f, _grid.Bounds.Width, _grid.Bounds.Height)));
        _moveShadow = new(_map.Bounds) { BackColor = Colors.SlateGray.WithAlpha(.7f) };
        _attackShadow = new(_map.Bounds) { BackColor = Colors.Crimson.WithAlpha(.4f) };
        _miniMap = Add(new MiniMapComponent(MiniMapClicked, new(1200f, 500f, 100f, 100f))
        {
            IgnoreCamera = true
        });

        var buttonTop = 0f;
        _attackButton = Add(new ButtonComponent(new(1200f, buttonTop += 50f, 100f, 50f), "Attack", OnAttack) { IgnoreCamera = true });
        _waitButton = Add(new ButtonComponent(new(1200f, buttonTop += 60f, 100f, 50f), "Wait", OnWait) { IgnoreCamera = true });

        _map.AddChild(_moveShadow);
        _map.AddChild(_attackShadow);
        _map.AddChild(_grid);

        var units = new[]
        {
            _map.AddChild(new BattleUnitComponent(Images.ArcherIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.HealerIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.MageIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.NinjaIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.WarriorIdle01, new(0))),
            _map.AddChild(new BattleUnitComponent(Images.ArcherIdle02, new(1))),
            _map.AddChild(new BattleUnitComponent(Images.HealerIdle02, new(1))),
            _map.AddChild(new BattleUnitComponent(Images.MageIdle02, new(1))),
            _map.AddChild(new BattleUnitComponent(Images.NinjaIdle02, new(1))),
            _map.AddChild(new BattleUnitComponent(Images.WarriorIdle02, new(1)))
        };

        InitializeBattlefield(units);
    }

    private void InitializeBattlefield(BattleUnitComponent[] units)
    {
        // temporary solution
        _battleUnits = units.OrderBy(_ => Guid.NewGuid()).ToList();

        foreach (var (unit, index) in _battleUnits.Where(x => x.State.PlayerId == 0).Select((x, i) => (x, i)))
        {
            unit.Position = _grid.GetPositionForTile(1, (index * 2) + 1);
        }

        foreach (var (unit, index) in _battleUnits.Where(x => x.State.PlayerId == 1).Select((x, i) => (x, i)))
        {
            unit.Position = _grid.GetPositionForTile(8, (index * 2) + 1);
        }

        ActiveCamera.Offset = new PointF(600f, 100f);
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
        _cameraTween = ActiveCamera.Offset.TweenTo(target, 100f);
    }

    public void GridClicked(int x, int y)
    {
        var position = _grid.GetPositionForTile(x, y);
        if (!_moveShadow.Shadows.Any(x => x.Contains(position)))
            return;

        var finalTarget = _grid.GetPositionForTile(x, y);
        _gridTween = CurrentUnit.Position.TweenTo(200f, finalTarget);
    }

    public override void Update(float deltaTime)
    {
        if (_gridTween is not null)
            CurrentUnit.Position = _gridTween.Advance(deltaTime);

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
                .CreateFanOutArea(_grid.GetTileForPosition(CurrentUnit.Position), new(_grid.ColCount, _grid.RowCount), CurrentUnit.State.AttackRange)
                .Where(x => !gridToUnit.Contains(x) || gridToUnit[x].All(y => y.State.PlayerId != currentUnit.State.PlayerId))
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
        _gridTween = null;
    }
}
