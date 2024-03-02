using Rpg.Mobile.App.Game.Battling.Components;
using Rpg.Mobile.App.Game.Menu;
using Rpg.Mobile.GameEngine.Scenes.Battling.Rules.Models;
using Rpg.Mobile.GameSdk;
using static Rpg.Mobile.App.Game.Sprites;

namespace Rpg.Mobile.App.Game.Battling.Scenes;

public class BattleGridScene : SceneBase
{
    private readonly MapComponent _map;
    private readonly MiniMapComponent _miniMap;
    private readonly ButtonComponent _attackButton;
    private readonly ButtonComponent _waitButton;
    private readonly TileShadowComponent _moveShadow;

    private readonly List<BattleUnitComponent> _battleUnits;
    private ITween<PointF>? _movingUnit;

    public BattleGridScene()
    {
        _map = Add(new MapComponent(GridClicked, new(0f, 0f, 200f, 200f)));
        _miniMap = Add(new MiniMapComponent(ActiveCamera, new(1200f, 500f, 100f, 100f)) { IgnoreCamera = true });

        var buttonTop = 0f;
        _attackButton = Add(new ButtonComponent(new(1200f, buttonTop += 50f, 100f, 50f), "Attack", OnAttack) { IgnoreCamera = true });
        _waitButton = Add(new ButtonComponent(new(1200f, buttonTop += 60f, 100f, 50f), "Wait", OnWait) { IgnoreCamera = true });
        _moveShadow = Add(new TileShadowComponent(_map.Bounds));

        _battleUnits = new()
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

        foreach (var (unit, index) in _battleUnits.Where(x => x.State.PlayerId == 0).Select((x, i) => (x, i)))
        {
            unit.MoveTo(_map.Grid.GetPositionForTile(1, (index * 2) + 1));
        }

        foreach (var (unit, index) in _battleUnits.Where(x => x.State.PlayerId == 1).Select((x, i) => (x, i)))
        {
            unit.MoveTo(_map.Grid.GetPositionForTile(9, (index * 2) + 1));
        }

        ActiveCamera.Offset = new PointF(-10f, -30f);
    }

    public void OnAttack(IEnumerable<PointF> touches)
    {

    }

    public void OnWait(IEnumerable<PointF> touches)
    {
    }

    public void GridClicked(int x, int y)
    {
        var unit = _battleUnits[0];
        var currentY = (int)(unit.Position.Y / _map.Grid.Size);
        var horizontalTarget = _map.Grid.GetPositionForTile(x, currentY);
        var finalTarget = _map.Grid.GetPositionForTile(x, y);

        _movingUnit = _battleUnits[0].Position.TweenTo(200f, horizontalTarget, finalTarget);
    }

    public override void Update(float deltaTime)
    {
        if (_movingUnit is not null)
            _battleUnits[0].MoveTo(_movingUnit.Advance(deltaTime));
    }
}
