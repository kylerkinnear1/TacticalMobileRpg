using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Calculators;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active;

public class ActivePhase(
    IEventBus _bus,
    BattleData _data,
    ISelectingAttackTargetCalculator _attackTargetCalculator,
    ISelectingMagicTargetCalculator _magicTargetCalculator,
    IPathCalculator _path) : IBattlePhase
{
    public interface IStep : IState { }

    private ISubscription[] _subscriptions = [];
    private readonly StateMachine<IStep> _step = new();
    
    public record MagicClickedEvent : IEvent;
    public record AttackClickedEvent : IEvent;
    
    public record CompletedEvent(BattleUnitData Unit) : IEvent;
    public record NotEnoughMpEvent(SpellData Spell) : IEvent;
    public record SpellSelectedEvent(SpellData Spell) : IEvent;
    
    public void Enter()
    {
        _subscriptions =
        [
            _bus.Subscribe<AttackClickedEvent>(_ => _step.Change(new SelectingAttackTargetStep(
                _data, 
                _bus,
                _attackTargetCalculator,
                _path))),
            _bus.Subscribe<MagicClickedEvent>(_ => _step.Change(new SelectingSpellStep(_data, _bus))),
            _bus.Subscribe<ActivePhaseBackClickedEvent>(BackClicked),
            _bus.Subscribe<SpellSelectedEvent>(_ => _step.Change(new SelectingMagicTargetStep(
                _data,
                _bus,
                _magicTargetCalculator,
                _path))),
            _bus.Subscribe<IdleStep.CompletedEvent>(evnt => _bus.Publish(new CompletedEvent(evnt.CurrentUnit)))
        ];
        
        _data.Active.ActiveUnitStartPosition = _data.UnitCoordinates[_data.CurrentUnit()];
        _step.Change(new IdleStep(_data, _bus, _path));
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
        _step.Change(null);
        _subscriptions.DisposeAll();
    }

    private void BackClicked(ActivePhaseBackClickedEvent evnt)
    {
        _data.UnitCoordinates[_data.CurrentUnit()] = _data.Active.ActiveUnitStartPosition;
    }
}