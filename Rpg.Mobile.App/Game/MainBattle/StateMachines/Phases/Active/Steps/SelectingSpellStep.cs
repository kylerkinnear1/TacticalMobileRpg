using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.StateManagement;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;

public class SelectingSpellStep(Context _context) : ActivePhase.IStep
{
    public void Enter()
    {
        var buttons = _context.Data.CurrentUnit.Spells
            .Select(x => new ButtonData(x.Name, _ => SpellSelected(x)))
            .Append(new("Back", _ => Bus.Global.Publish(new ActivePhase.BackClickedEvent())))
            .ToArray();

        _context.Menu.SetButtons(buttons);
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }

    private void SpellSelected(SpellData spell)
    {
        if (spell.MpCost > _context.Data.CurrentUnit.RemainingMp)
        {
            Bus.Global.Publish(new ActivePhase.NotEnoughMpEvent(spell));
            return;
        }

        _context.Data.CurrentSpell = spell;
        Bus.Global.Publish(new ActivePhase.SpellSelectedEvent(spell));
    }
}


