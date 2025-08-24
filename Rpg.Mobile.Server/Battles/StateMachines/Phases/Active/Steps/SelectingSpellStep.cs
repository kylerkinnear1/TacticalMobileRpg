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