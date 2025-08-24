using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases;

public class BattlePhaseMachine
{
    private readonly BattleData _data;
    private readonly MainBattleComponent _mainBattle;
    private readonly BattleMenuComponent _menu;
    private readonly IEventBus _bus;
    
    private ISubscription[] _subscriptions = [];
    private readonly StateMachine<IBattlePhase> _phase = new();

    public BattlePhaseMachine(
        BattleData data,
        MainBattleComponent mainBattle,
        BattleMenuComponent menu,
        IEventBus bus)
    {
        _data = data;
        _mainBattle = mainBattle;
        _menu = menu;
        _bus = bus;
    }

    public void Execute(float deltaTime) => _phase.Execute(deltaTime);
    
    public void Start() => 
        _subscriptions =
        [
            _bus.Subscribe<BattleNetwork.SetupStartedEvent>(SetupStarted),
            _bus.Subscribe<BattleNetwork.NewRoundStartedEvent>(NewRoundStarted),
            _bus.Subscribe<BattleNetwork.ActivePhaseStartedEvent>(ActivePhaseStarted),
        ];

    public void Stop() => _subscriptions.DisposeAll();

    private void SetupStarted(BattleNetwork.SetupStartedEvent evnt)
    {
        _data.Setup = evnt.SetupData;
        _data.Units = evnt.Units;
        _phase.Change(new Setup.SetupPhase(_data, _mainBattle, _bus));
    }
    
    private void NewRoundStarted(BattleNetwork.NewRoundStartedEvent evnt)
    {
        _data.Active.ActiveUnitIndex = evnt.ActiveUnitIndex;
        _data.Active.TurnOrderIds = evnt.TurnOrderIds;
    }
    
    private void ActivePhaseStarted(BattleNetwork.ActivePhaseStartedEvent evnt)
    {
        _data.Active = evnt.ActivePhaseData;
        _phase.Change(new ActivePhase(_data, _mainBattle, _menu, _bus));
    }
}