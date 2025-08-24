using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public class SelectingSpellStep(
    BattleData _data,
    IEventBus _bus) : ActivePhase.IStep
{
    public record StartedEvent(List<SpellData> Spells) : IEvent;

    private ISubscription[] _subscriptions = [];
    
    public void SpellSelected(SpellData spell)
    {
        if (spell.MpCost > _data.CurrentUnit().RemainingMp)
        {
            _bus.Publish(new ActivePhase.NotEnoughMpEvent(spell));
            return;
        }

        _data.Active.CurrentSpell = spell;
        _bus.Publish(new ActivePhase.SpellSelectedEvent(0, spell.Type));
    }

    public void Enter()
    {
        _bus.Publish(new StartedEvent(_data.CurrentUnit().Spells));
    }
    
    public void Execute(float deltaTime) { }

    public void Leave()
    {
        _subscriptions.DisposeAll();
    }
}