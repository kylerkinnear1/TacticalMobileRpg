using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using Rpg.Mobile.Server.Battles.Calculators;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Active;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Damage;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.NewRound;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Setup;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases;

public interface IBattlePhase : IState { }
public class BattlePhaseMachine : IDisposable
{
    private readonly BattleData _data;
    private readonly IEventBus _bus;
    private readonly IPathCalculator _path;
    private readonly ISelectingAttackTargetCalculator _attackTargetCalculator;
    private readonly ISelectingMagicTargetCalculator _magicTargetCalculator;
    private readonly IMagicDamageCalculator _magicDamageCalc;
    private readonly IAttackDamageCalculator _attackDamageCalc;
    
    private readonly ISubscription[] _subscriptions;
    private readonly StateMachine<IBattlePhase> _phase = new();

    public BattlePhaseMachine(
        BattleData data,
        IEventBus bus,
        IPathCalculator path,
        ISelectingAttackTargetCalculator attackTargetCalculator,
        ISelectingMagicTargetCalculator magicTargetCalculator,
        IMagicDamageCalculator magicDamageCalc,
        IAttackDamageCalculator attackDamageCalc)
    {
        _data = data;
        _bus = bus;
        _path = path;
        _attackTargetCalculator = attackTargetCalculator;
        _magicTargetCalculator = magicTargetCalculator;
        _magicDamageCalc = magicDamageCalc;
        _attackDamageCalc = attackDamageCalc;

        _subscriptions =
        [
            _bus.Subscribe<SetupPhase.CompletedEvent>(_ => StartFirstRound()),
            _bus.Subscribe<ActivePhase.CompletedEvent>(_ => UnitTurnEnded()),
            _bus.Subscribe<SelectingAttackTargetStep.AttackTargetSelectedEvent>(ApplyDamage),
            _bus.Subscribe<SelectingMagicTargetStep.MagicTargetSelectedEvent>(ApplyDamage),
            _bus.Subscribe<DamagePhase.CompletedEvent>(_ => UnitTurnEnded())
        ];
    }

    public void Change(IBattlePhase phase) => _phase.Change(phase);
    public void Execute(float deltaTime) => _phase.Execute(deltaTime);
    
    private void StartFirstRound()
    {
        _phase.Change(new NewRoundPhase(_data));
        _phase.Change(new ActivePhase(_bus, _data, _attackTargetCalculator, _magicTargetCalculator, _path));
    }

    private void UnitTurnEnded()
    {
        _data.Active.ActiveUnitIndex = (_data.Active.ActiveUnitIndex + 1) % _data.Active.TurnOrder.Count;

        if (_data.Active.ActiveUnitIndex == 0)
            _phase.Change(new NewRoundPhase(_data));

        _phase.Change(new ActivePhase(_bus, _data, _attackTargetCalculator, _magicTargetCalculator, _path));
    }

    private void ApplyDamage(SelectingAttackTargetStep.AttackTargetSelectedEvent evnt)
    {
        var phase = new DamagePhase(_data, _path, _attackDamageCalc, _magicDamageCalc, _bus);
        _phase.Change(phase);
        phase.PerformAttack(evnt);
    }

    private void ApplyDamage(SelectingMagicTargetStep.MagicTargetSelectedEvent evnt)
    {
        var phase = new DamagePhase(_data, _path, _attackDamageCalc, _magicDamageCalc, _bus);
        _phase.Change(phase);
        phase.CastSpell(evnt);
    }

    public void Dispose() => _subscriptions.DisposeAll();
}