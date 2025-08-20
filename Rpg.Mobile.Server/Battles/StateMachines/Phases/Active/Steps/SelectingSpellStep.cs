using Rpg.Mobile.Api.Battles.Data;
using static Rpg.Mobile.Server.Battles.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public class SelectingSpellStep(Context _context) : ActivePhase.IStep
{
    public void SpellSelected(SpellData spell)
    {
        if (spell.MpCost > _context.Data.CurrentUnit().RemainingMp)
        {
            Bus.Global.Publish(new ActivePhase.NotEnoughMpEvent(spell));
            return;
        }

        _context.Data.Active.CurrentSpell = spell;
        Bus.Global.Publish(new ActivePhase.SpellSelectedEvent(spell));
    }
}