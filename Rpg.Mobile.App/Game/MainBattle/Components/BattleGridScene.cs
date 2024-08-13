using Rpg.Mobile.App.Game.MainBattle.Calculators;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.App.Game.MainBattle.StateMachines;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Setup;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.Inputs;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Tweening;

namespace Rpg.Mobile.App.Game.MainBattle.Components;

public class BattleGridScene : SceneBase
{
    private readonly MainBattleComponent _battle;
    private readonly MiniMapComponent _miniMap;
    private readonly BattleMenuComponent _battleMenu;
    private readonly StatSheetComponent _stats;
    private readonly MouseCoordinateComponent _mouseComponent;
    private readonly TextboxComponent _hoverComponent;
    private readonly BattlePhaseMachine _stateMachine;

    private ITween<PointF>? _cameraTween;
    
    public BattleGridScene(IMouse mouse)
    {
        var mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.json");
        var jsonLoader = new FileReader();
        var mapJson = jsonLoader.ReadJson<MapJson>(mapPath);
        var mapState = mapJson.ToState();
        var battleData = new BattleData(mapState);
        
        Add(_battle = new(new(0f, 0f), battleData));
        Add(_battleMenu = new(new(900f, 100f, 150f, 200f)));
        Add(_miniMap = new(new(_battleMenu.Bounds.Right + 100f, _battleMenu.Bounds.Bottom + 100f, 200f, 200f)) { IgnoreCamera = true });
        Add(_stats = new(battleData, new(900f, _battleMenu.Bounds.Bottom + 30f, 150, 300f)) { IgnoreCamera = true });

        Add(_mouseComponent = new(mouse, new(_miniMap.AbsoluteBounds.Left, _miniMap.AbsoluteBounds.Bottom, 300f, 100f))
        {
            IgnoreCamera = true
        });

        Add(_hoverComponent = new(new(_stats.Bounds.Left, _stats.Bounds.Bottom + 100f, 300f, 200f))
        {
            IgnoreCamera = true,
            BackColor = Colors.DeepSkyBlue
        });

        var context = new BattlePhaseMachine.Context(battleData, _battle, _battleMenu, new PathCalculator());
        _stateMachine = new BattlePhaseMachine(context);

        Bus.Global.Subscribe<TileHoveredEvent>(x => _hoverComponent.Label = $"{x.Tile.X}x{x.Tile.Y}");
        Bus.Global.Subscribe<MiniMapClickedEvent>(MiniMapClicked);

        _stateMachine.Change(new SetupPhase(context));
        ActiveCamera.Offset = new PointF(80f, 80f);
    }

    public override void Update(float deltaTime)
    {
        if (_cameraTween is not null)
            ActiveCamera.Offset = _cameraTween.Advance(deltaTime);

        _stateMachine.Execute(deltaTime);
    }

    private void MiniMapClicked(MiniMapClickedEvent touch)
    {
        var xPercent = touch.Position.X / _miniMap.Bounds.Width;
        var yPercent = touch.Position.Y / _miniMap.Bounds.Height;
        var target = new PointF(ActiveCamera.Size.Width * xPercent * 2, ActiveCamera.Size.Height * yPercent * 2);
        _cameraTween = ActiveCamera.Offset.SpeedTween(target, 1000f);
    }
}
