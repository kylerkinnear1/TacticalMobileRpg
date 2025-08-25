using Rpg.Mobile.Api.Battles;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.MainBattle.Components;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active;
using Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Damage;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases;

public class BattlePhaseMachine
{
    private readonly BattleData _data;
    private readonly MainBattleComponent _mainBattle;
    private readonly BattleMenuComponent _menu;
    private readonly IEventBus _bus;
    private readonly ISelectingAttackTargetCalculator _attackTargetCalculator;
    private readonly ISelectingMagicTargetCalculator _magicTargetCalculator;

    private ISubscription[] _subscriptions = [];
    private readonly StateMachine<IBattlePhase> _phase = new();

    public BattlePhaseMachine(
        BattleData data,
        MainBattleComponent mainBattle,
        BattleMenuComponent menu,
        IEventBus bus,
        ISelectingAttackTargetCalculator attackTargetCalculator,
        ISelectingMagicTargetCalculator magicTargetCalculator)
    {
        _data = data;
        _mainBattle = mainBattle;
        _menu = menu;
        _bus = bus;
        _attackTargetCalculator = attackTargetCalculator;
        _magicTargetCalculator = magicTargetCalculator;
    }

    public void Execute(float deltaTime) => _phase.Execute(deltaTime);

    public void Start() =>
        _subscriptions =
        [
            _bus.Subscribe<BattleNetwork.SetupStartedEvent>(SetupStarted),
            _bus.Subscribe<BattleNetwork.NewRoundStartedEvent>(NewRoundStarted),
            _bus.Subscribe<BattleNetwork.ActivePhaseStartedEvent>(ActivePhaseStarted),
            _bus.Subscribe<IBattleEventApi.UnitsDamagedEvent>(UnitsDamaged)
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
        _phase.Change(new ActivePhase(_data, _mainBattle, _menu, _bus, _attackTargetCalculator,
            _magicTargetCalculator));
    }
    
    private void UnitsDamaged(IBattleEventApi.UnitsDamagedEvent evnt)
    {
        foreach (var unit in evnt.DamagedUnits)
        {
            var unitIndex = _data.Units.FindIndex(x => x.UnitId == unit.Unit.UnitId);
            _data.Units[unitIndex] = unit.Unit;
        }

        foreach (var unit in evnt.DefeatedUnits)
        {
            var unitIndex = _data.Units.FindIndex(x => x.UnitId == unit.UnitId);
            _data.Units[unitIndex] = unit;
        }

        _data.UnitCoordinates = evnt.UnitCoordinates;
        _data.Active.ActiveUnitIndex = evnt.ActiveActiveUnitIndex;
        _data.CurrentUnit().RemainingMp = evnt.RemainingMp;
        _data.Active.TurnOrderIds = evnt.ActiveTurnOrderIds;
        _phase.Change(new DamagePhase(_mainBattle, _bus, _data));
    }
}