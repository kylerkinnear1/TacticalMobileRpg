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
    public interface IStep : IState
    {
    }

    private ISubscription[] _subscriptions = [];
    private readonly StateMachine<IStep> _step = new();

    public record MagicClickedEvent(int PlayerId) : IEvent;
    public record AttackClickedEvent(int PlayerId) : IEvent;
    public record CompletedEvent(BattleUnitData Unit) : IEvent;
    public record NotEnoughMpEvent(SpellData Spell) : IEvent;
    public record SpellSelectedEvent(int PlayerId, SpellType Spell) : IEvent;
    public record ActiveSpellChangedEvent(SpellData Spell) : IEvent;
    public record UnitMovedEvent(Point Tile) : IEvent;
    public record BackClickedEvent(int PlayerId) : IEvent;
    
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
            _bus.Subscribe<BackClickedEvent>(BackClicked),
            _bus.Subscribe<SpellSelectedEvent>(SpellSelected),
            _bus.Subscribe<IdleStep.CompletedEvent>(evnt => _bus.Publish(new CompletedEvent(evnt.CurrentUnit)))
        ];

        _data.Active.ActiveUnitStartPosition = _data.UnitCoordinates[_data.CurrentUnit().UnitId];
        _step.Change(new IdleStep(_data, _bus, _path));
    }

    private void SpellSelected(SpellSelectedEvent evnt)
    {
        var spell = _data.CurrentUnit().Spells.FirstOrDefault(x => x.Type == evnt.Spell);
        if (spell == null)
        {
            return;
        }
        
        _data.Active.CurrentSpell = spell;
        
        _bus.Publish(new ActiveSpellChangedEvent(spell));
        _step.Change(new SelectingMagicTargetStep(
            _data,
            _bus,
            _magicTargetCalculator,
            _path));
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
        _step.Change(null);
        _subscriptions.DisposeAll();
    }

    private void BackClicked(BackClickedEvent evnt)
    {
        _data.UnitCoordinates[_data.CurrentUnit().PlayerId] = _data.Active.ActiveUnitStartPosition;
        _bus.Publish(new UnitMovedEvent(_data.Active.ActiveUnitStartPosition));
    }
}