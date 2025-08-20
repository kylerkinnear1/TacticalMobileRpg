using Rpg.Mobile.Api.Battles.Data;
using static Rpg.Mobile.Server.Battles.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

public class SelectingSpellStepServer(Context _context) : ActivePhaseServer.IStep
{
    private void SpellSelected(SpellData spell)
    {
        if (spell.MpCost > _context.Data.CurrentUnit().RemainingMp)
        {
            Bus.Global.Publish(new ActivePhaseServer.NotEnoughMpEvent(spell));
            return;
        }

        _context.Data.Active.CurrentSpell = spell;
        Bus.Global.Publish(new ActivePhaseServer.SpellSelectedEvent(spell));
    }
}

public class SelectingSpellStepClient
{
    public void Enter()
    {
        var buttons = _context.Data.CurrentUnit().Spells
            .Select(x => new ButtonData(x.Name, _ => SpellSelected(x)))
            .Append(new("Back", _ => Bus.Global.Publish(new ActivePhaseServer.BackClickedEvent())))
            .ToArray();

        _context.Menu.SetButtons(buttons);
    }
}


