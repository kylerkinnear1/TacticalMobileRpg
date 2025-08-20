using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active;

public class ActivePhase(BattlePhaseMachine.Context _context) : IBattlePhase
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
            Bus.Global.Subscribe<AttackClickedEvent>(_ => _step.Change(new SelectingAttackTargetStep(_context))),
            Bus.Global.Subscribe<MagicClickedEvent>(_ => _step.Change(new SelectingSpellStep(_context))),
            Bus.Global.Subscribe<ActivePhaseBackClickedEvent>(BackClicked),
            Bus.Global.Subscribe<SpellSelectedEvent>(_ => _step.Change(new SelectingMagicTargetStep(_context))),
            Bus.Global.Subscribe<IdleStep.CompletedEvent>(evnt => Bus.Global.Publish(new CompletedEvent(evnt.CurrentUnit)))
        ];
        
        _context.Data.Active.ActiveUnitStartPosition = _context.Data.UnitCoordinates[_context.Data.CurrentUnit()];
        _step.Change(new IdleStep(_context));
    }

    public void Leave()
    {
        _step.Change(null);
        _subscriptions.DisposeAll();
    }

    private void BackClicked(ActivePhaseBackClickedEvent evnt)
    {
        _context.Data.UnitCoordinates[_context.Data.CurrentUnit()] = _context.Data.Active.ActiveUnitStartPosition;
    }
}