using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.Networking;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.Lobby;

public class LobbyScene : SceneBase
{
    public record JoinGameClickedEvent(List<BattleUnitType> Team) : IEvent;

    private readonly ButtonComponent _joinGameButton;
    private readonly TextboxComponent _titleText;
    private readonly TextboxComponent _statusText;
    private readonly IEventBus _bus;
    private readonly GameSettings _settings;
    private readonly NetworkMonitorComponent _networkMonitor;
    private readonly IGameLoop _game;

    public LobbyScene( 
        IEventBus bus, 
        IGameLoop game,
        GameSettings settings)
    {
        _bus = bus;
        _settings = settings;
        _game = game;
        
        Add(_titleText = new TextboxComponent(
            new RectF(200f, 100f, 400f, 60f), 
            "Curator's Collection")
        {
            FontSize = 32f,
            TextColor = Colors.White,
            BackColor = Colors.Transparent,
            IgnoreCamera = true
        });

        Add(_statusText = new TextboxComponent(
            new RectF(200f, 200f, 400f, 40f), 
            "Ready to join game")
        {
            FontSize = 16f,
            TextColor = Colors.Gray,
            BackColor = Colors.Transparent,
            IgnoreCamera = true
        });

        Add(_joinGameButton = new ButtonComponent(
            new RectF(300f, 300f, 200f, 60f),
            "Join Game",
            OnJoinGameClicked)
        {
            BackColor = Colors.DarkBlue,
            TextColor = Colors.White,
            FontSize = 18f,
            IgnoreCamera = true
        });
        
        Add(_networkMonitor = new NetworkMonitorComponent(
            bus,
            game,
            new RectF(10f, 400f, 400f, 350f),
            new BattleData())
        {
            IgnoreCamera = true
        });
    }
    private void OnJoinGameClicked(IEnumerable<PointF> touches)
    {
        _statusText.Label = "Connecting...";
        _joinGameButton.Visible = false;
        _bus.Publish(new JoinGameClickedEvent(_settings.Team.ToList()));
    }
    
    public override void Update(float deltaTime)
    {
    }
}