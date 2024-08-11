using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Game.MainBattle.Data;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.GameSdk.StateManagement;
using static Rpg.Mobile.App.Game.MainBattle.States.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.Active.Steps;

public class SelectingSpellStep : ActivePhase.IStep
{
    private readonly Context _context;

    public SelectingSpellStep(Context context) => _context = context;

    public void Enter()
    {
        var buttons = _context.Data.CurrentUnit.Spells.Select<SpellData, ButtonData>(x => new ButtonData(x.Name, _ => Bus.Global.Publish(new SpellSelectedEvent(x))))
            .Append(new("Back", _ => Bus.Global.Publish(new BackClickedEvent())))
            .ToArray();

        _context.Menu.SetButtons(buttons);
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}


