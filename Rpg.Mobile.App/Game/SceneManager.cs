using Rpg.Mobile.App.Game.Lobby;
using Rpg.Mobile.App.Game.MainBattle;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game;

public class SceneManager : IDisposable
{
    private readonly LobbyScene _lobby;
    private readonly BattleGridScene _battle;
    private readonly IGameLoop _game;
    private readonly IEventBus _bus;

    private readonly ISubscription[] _subscriptions;

    public SceneManager(LobbyScene lobby, BattleGridScene battle, IGameLoop game, IEventBus bus)
    {
        _lobby = lobby;
        _battle = battle;
        _game = game;
        _bus = bus;

        _subscriptions = 
        [
            _bus.Subscribe<LobbyNetwork.GameStartedEvent>(OnGameStarted)
        ];
    }
    
    private void OnGameStarted(LobbyNetwork.GameStartedEvent evnt)
    {
        _game.ChangeScene(_battle);
    }

    public void Dispose() => _subscriptions.DisposeAll();
}