using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Api.Lobby;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Lobby;

public class LobbyNetwork
{
    public record GameStartedEvent(string GameId, BattleData Battle) : IEvent;
    public record GameEndedEvent(string GameId) : IEvent;
    
    private readonly ILobbyClient _lobbyClient;
    private readonly IEventBus _bus;
    private ISubscription[] _subscriptions = [];
    private readonly IGameLoop _game;

    public LobbyNetwork(
        ILobbyClient lobbyClient,
        IEventBus bus,
        IGameLoop game)
    {
        _lobbyClient = lobbyClient;
        _bus = bus;
        _game = game;
    }
    
    public void Connect()
    {
        _lobbyClient.GameStarted += GameStarted;
        _lobbyClient.GameEnded += GameEnded;
        
        _subscriptions =
        [
            _bus.Subscribe<LobbyScene.JoinGameClickedEvent>(JoinGameClicked)
        ];
    }

    public void Disconnect()
    {
        _lobbyClient.GameStarted -= GameStarted;
        _lobbyClient.GameEnded -= GameEnded;
        _subscriptions.DisposeAll();
    }

    private void GameStarted(string gameId, BattleData battle)
    {
        _game.Execute(() => _bus.Publish(new GameStartedEvent(gameId, battle)));
    }

    private void GameEnded(string gameId)
    {
        _game.Execute(() => _bus.Publish(new GameEndedEvent(gameId)));
    }

    private void JoinGameClicked(LobbyScene.JoinGameClickedEvent evnt)
    {
        _lobbyClient.ConnectToGame(evnt.GameId, evnt.Team);
    }

    public void Dispose()
    {
        _lobbyClient.GameStarted -= GameStarted;
        _subscriptions.DisposeAll();
    }
}