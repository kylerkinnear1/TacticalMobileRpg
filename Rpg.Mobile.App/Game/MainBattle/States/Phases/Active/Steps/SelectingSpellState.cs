using Rpg.Mobile.App.Game.Common;
using Rpg.Mobile.App.Game.MainBattle.Events;
using Rpg.Mobile.GameSdk.StateManagement;
using static Rpg.Mobile.App.Game.MainBattle.BattlePhaseStateMachine;

namespace Rpg.Mobile.App.Game.MainBattle.States.ActivePhase;

public class SelectingSpellPhase : IBattlePhase
{
    private readonly Context _context;

    public SelectingSpellPhase(Context context) => _context = context;

    public void Enter()
    {
        var buttons = _context.Data.CurrentUnit.Spells
            .Select(x => new ButtonData(x.Name, _ => Bus.Global.Publish(new SpellSelectedEvent(x))))
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


