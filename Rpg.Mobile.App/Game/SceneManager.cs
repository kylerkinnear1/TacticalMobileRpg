using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.App.Game.Lobby;
using Rpg.Mobile.App.Game.MainBattle;
using Rpg.Mobile.GameSdk.Core;
using Rpg.Mobile.GameSdk.Inputs;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game;

public class SceneManager
{
    private readonly LobbyScene _lobby;
    private readonly IGameLoop _game;
    private readonly IEventBus _bus;
    private readonly IMouse _mouse;
    private readonly IPathCalculator _path;

    private ISubscription[] _subscriptions = [];

    public SceneManager(
        LobbyScene lobby,
        IGameLoop game,
        IEventBus bus,
        IMouse mouse,
        IPathCalculator path)
    {
        _lobby = lobby;
        _game = game;
        _bus = bus;
        _mouse = mouse;
        _path = path;
    }

    public void Subscribe()
    {
        _subscriptions = 
        [
            _bus.Subscribe<LobbyNetwork.GameStartedEvent>(e =>
            {
                var battleScene = new BattleGridScene(_mouse, e.Battle, _bus, _path);
                _game.ChangeScene(battleScene);
            }),
            _bus.Subscribe<LobbyNetwork.GameEndedEvent>(_ => _game.ChangeScene(_lobby))
        ];
    }

    public void Unsubscribe()
    {
        _subscriptions.DisposeAll();
    }
}