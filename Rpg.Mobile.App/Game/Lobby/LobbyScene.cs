using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Api.Lobby;
using Rpg.Mobile.App.Game.MainBattle;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.Lobby.LobbyScene;

namespace Rpg.Mobile.App.Game.Lobby;

public class LobbyScene : SceneBase
{
    public record JoinGameClickedEvent(string GameId) : IEvent;

    private readonly ButtonComponent _joinGameButton;
    private readonly TextboxComponent _titleText;
    private readonly TextboxComponent _statusText;
    private readonly IEventBus _bus;
    private readonly GameSettings _settings;
    private readonly IGameLoop _game;
    private readonly BattleGridScene _battleScene;

    public LobbyScene(
        IGameLoop game, 
        IEventBus bus, 
        GameSettings settings,
        BattleGridScene battleScene)
    {
        _game = game;
        _bus = bus;
        _settings = settings;
        _battleScene = battleScene;
        
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

public class LobbyNetwork : IDisposable
{
    public record GameStartedEvent(string GameId, BattleData Battle) : IEvent;
    
    private readonly ILobbyClient _lobbyClient;
    private readonly IEventBus _bus;
    private readonly ISubscription[] _subscriptions;
    private readonly IGameLoop _game;

    public LobbyNetwork(
        ILobbyClient lobbyClient,
        IEventBus bus,
        IGameLoop game)
    {
        _lobbyClient = lobbyClient;
        _bus = bus;
        _game = game;

        _lobbyClient.GameStarted += GameStarted;
        
        _subscriptions =
        [
            _bus.Subscribe<JoinGameClickedEvent>(JoinGameClicked)
        ];
    }

    private void GameStarted(string gameId, BattleData battle)
    {
        _game.Execute(() => _bus.Publish(new GameStartedEvent(gameId, battle)));
    }

    private void JoinGameClicked(JoinGameClickedEvent evnt)
    {
        _lobbyClient.ConnectToGame(evnt.GameId);
    }

    public void Dispose()
    {
        _lobbyClient.GameStarted -= GameStarted;
        _subscriptions.DisposeAll();
    }
}
