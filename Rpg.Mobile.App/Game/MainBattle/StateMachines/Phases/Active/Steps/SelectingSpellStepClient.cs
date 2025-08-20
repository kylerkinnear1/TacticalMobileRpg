using Rpg.Mobile.App.Game.UserInterface;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.Active.Steps;

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