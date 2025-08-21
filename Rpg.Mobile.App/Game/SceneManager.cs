using Rpg.Mobile.App.Game.Lobby;
using Rpg.Mobile.App.Game.MainBattle;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game;

public class SceneManager
{
    private readonly LobbyScene _lobby;
    private readonly BattleGridScene _battle;
    private readonly IGameLoop _game;
    private readonly IEventBus _bus;

    private ISubscription[] _subscriptions = [];

    public SceneManager(LobbyScene lobby, BattleGridScene battle, IGameLoop game, IEventBus bus)
    {
        _lobby = lobby;
        _battle = battle;
        _game = game;
        _bus = bus;
    }

    public void Subscribe()
    {
        _subscriptions = 
        [
            _bus.Subscribe<LobbyNetwork.GameStartedEvent>(_ => _game.ChangeScene(_battle)),
            _bus.Subscribe<LobbyNetwork.GameEndedEvent>(_ => _game.ChangeScene(_lobby))
        ];
    }

    public void Unsubscribe()
    {
        _subscriptions.DisposeAll();
    }
}