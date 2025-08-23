using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases;

public class BattlePhaseMachine : IDisposable
{
    private readonly BattleData _data;
    private readonly MainBattleComponent _mainBattle;
    private readonly IEventBus _bus;
    private readonly StateMachine<IBattlePhase> _phase = new();
    private ISubscription[] _subscriptions = [];

    public BattlePhaseMachine(
        BattleData data,
        MainBattleComponent mainBattle,
        IEventBus bus)
    {
        _data = data;
        _mainBattle = mainBattle;
        _bus = bus;
    }

    public void Execute(float deltaTime) => _phase.Execute(deltaTime);
    
    public void Start() => 
        _subscriptions =
        [
            _bus.Subscribe<BattleNetwork.SetupStartedEvent>(SetupStarted)
        ];

    public void Stop() => _subscriptions.DisposeAll();

    private void SetupStarted(BattleNetwork.SetupStartedEvent evnt)
    {
        _data.Setup = evnt.SetupData;
        _data.Units = evnt.Units;
        _phase.Change(new Setup.SetupPhase(_data, _mainBattle, _bus));
    }

    public void Dispose()
    {
        _subscriptions.DisposeAll();
    }
}