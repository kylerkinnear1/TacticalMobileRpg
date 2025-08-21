using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.Lobby;

public class LobbyScene : SceneBase
{
    public record JoinGameClickedEvent(string GameId) : IEvent;

    private readonly ButtonComponent _joinGameButton;
    private readonly TextboxComponent _titleText;
    private readonly TextboxComponent _statusText;
    private readonly IEventBus _bus;
    private readonly GameSettings _settings;

    public LobbyScene( 
        IEventBus bus, 
        GameSettings settings)
    {
        _bus = bus;
        _settings = settings;
        
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
    }
    private void OnJoinGameClicked(IEnumerable<PointF> touches)
    {
        _statusText.Label = "Connecting...";
        _joinGameButton.Visible = false;
        _bus.Publish(new JoinGameClickedEvent(_settings.GameId));
    }
    
    public override void Update(float deltaTime)
    {
    }
}