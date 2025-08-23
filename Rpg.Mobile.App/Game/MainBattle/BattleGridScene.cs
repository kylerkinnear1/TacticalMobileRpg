using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Setup;
using Rpg.Mobile.App.Game.Networking;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.Inputs;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Tweening;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle;

public class BattleGridScene : SceneBase
{
    private readonly MainBattleComponent _battle;
    private readonly MiniMapComponent _miniMap;
    private readonly BattleMenuComponent _battleMenu;
    private readonly StatSheetComponent _stats;
    private readonly MouseCoordinateComponent _mouseComponent;
    private readonly TextboxComponent _hoverComponent;
    private readonly NetworkMonitorComponent _networkMonitor;

    private readonly BattleData _data;
    private readonly IEventBus _bus;
    private readonly IPathCalculator _path;
    private readonly IGameLoop _game;

    private ITween<PointF>? _cameraTween;
    private ISubscription[] _subscriptions = [];
    
    private readonly BattlePhaseMachine _phase;

    public BattleGridScene(
        IGameLoop game,
        IMouse mouse,
        BattleData data,
        IEventBus bus,
        IPathCalculator path)
    {
        _data = data;
        _bus = bus;
        _path = path;
        _game = game;
        
        Add(_battle = new(new(0f, 0f), _path, _data, _bus));
        Add(_battleMenu = new(new(
            900f, 
            100f, 
            150f,
            200f)));
        Add(_miniMap = new(_bus, new(
            _battleMenu.Bounds.Right + 100f,
            _battleMenu.Bounds.Bottom + 100f, 
            200f, 
            200f))
        {
            IgnoreCamera = true
        });
        Add(_stats = new(_data, _bus, new(900f, _battleMenu.Bounds.Bottom + 30f, 150, 300f)) { IgnoreCamera = true });

        Add(_mouseComponent = new(mouse, new(_miniMap.AbsoluteBounds.Left, _miniMap.AbsoluteBounds.Bottom, 300f, 100f))
        {
            IgnoreCamera = true
        });

        Add(_hoverComponent = new(new(_stats.Bounds.Left, _stats.Bounds.Bottom + 100f, 300f, 200f))
        {
            IgnoreCamera = true,
            BackColor = Colors.DeepSkyBlue
        });
        
        Add(_networkMonitor = new NetworkMonitorComponent(
            bus, 
            game,
            new(
                _miniMap.Bounds.Left, 
                10f,  // Top of screen
                400f, // Width
                350f  // Height
            ))
        {
            IgnoreCamera = true
        });
        
        ActiveCamera.Offset = new PointF(80f, 80f);
        
        _phase = new BattlePhaseMachine(data, _battle, _bus);
    }

    public override void Update(float deltaTime)
    {
        if (_cameraTween is not null)
            ActiveCamera.Offset = _cameraTween.Advance(deltaTime);

        _phase.Execute(deltaTime);
    }

    private void MiniMapClicked(MiniMapComponent.MiniMapClickedEvent touch)
    {
        var xPercent = touch.Position.X / _miniMap.Bounds.Width;
        var yPercent = touch.Position.Y / _miniMap.Bounds.Height;
        var target = new PointF(ActiveCamera.Size.Width * xPercent * 2, ActiveCamera.Size.Height * yPercent * 2);
        _cameraTween = ActiveCamera.Offset.SpeedTween(target, 1000f);
    }

    public override void OnEnter()
    {
        _subscriptions =
        [
            _bus.Subscribe<GridComponent.TileHoveredEvent>(x => _hoverComponent.Label = $"{x.Tile.X}x{x.Tile.Y}"),
            _bus.Subscribe<MiniMapComponent.MiniMapClickedEvent>(MiniMapClicked)
        ];
        
        _phase.Start();
        base.OnEnter();
    }

    public override void OnExit()
    {
        _subscriptions.DisposeAll();
        _phase.Stop();
        
        base.OnExit();
    }
}
