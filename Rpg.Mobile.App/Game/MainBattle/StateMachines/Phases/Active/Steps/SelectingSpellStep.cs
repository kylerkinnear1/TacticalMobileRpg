using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.App.Game.UserInterface;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.Active.Steps;

public class SelectingSpellStep : ActivePhase.IStep
{
    public record SpellSelectedEvent(SpellData Spell) : IEvent;
    
    private readonly BattleData _data;
    private readonly MenuComponent _menu;
    private readonly IEventBus _bus;
    
    public void Enter()
    {
        var buttons = _data.CurrentUnit().Spells
            .Select(x => new ButtonData(x.Name, _ => _bus.Publish(new SpellSelectedEvent(x))))
            .Append(new("Back", _ => _bus.Publish(new ActivePhase.BackClickedEvent())))
            .ToArray();

        _menu.SetButtons(buttons);
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}