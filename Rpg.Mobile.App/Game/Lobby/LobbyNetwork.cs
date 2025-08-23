using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.Api.Lobby;
using Rpg.Mobile.App.Game.MainBattle;
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
    private readonly IGameLoop _game;
    private readonly GameSettings _settings;
    
    private ISubscription[] _subscriptions = [];

    public LobbyNetwork(
        ILobbyClient lobbyClient,
        IEventBus bus,
        IGameLoop game,
        GameSettings settings)
    {
        _lobbyClient = lobbyClient;
        _bus = bus;
        _game = game;
        _settings = settings;
    }
    
    public void Connect()
    {
        _lobbyClient.GameStarted += GameStarted;
        _lobbyClient.GameEnded += GameEnded;
        
        _subscriptions =
        [
            _bus.Subscribe<LobbyScene.JoinGameClickedEvent>(evnt => _lobbyClient.ConnectToGame(_settings.GameId, evnt.Team)),
            _bus.Subscribe<BattleGridScene.PlayerReadyEvent>(_ => _lobbyClient.PlayerReady(_settings.GameId))
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
}