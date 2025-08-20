using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Api.Lobby;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.Lobby;

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
            _bus.Subscribe<LobbyScene.JoinGameClickedEvent>(JoinGameClicked)
        ];
    }

    private void GameStarted(string gameId, BattleData battle)
    {
        _game.Execute(() => _bus.Publish(new GameStartedEvent(gameId, battle)));
    }

    private void JoinGameClicked(LobbyScene.JoinGameClickedEvent evnt)
    {
        _lobbyClient.ConnectToGame(evnt.GameId);
    }

    public void Dispose()
    {
        _lobbyClient.GameStarted -= GameStarted;
        _subscriptions.DisposeAll();
    }
}