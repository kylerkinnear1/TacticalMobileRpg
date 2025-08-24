using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.NewRound;

public class NewRoundPhase(
    BattleData _data,
    IEventBus _bus) : IBattlePhase
{
    public record StartedEvent(List<int> TurnOrderIds, int ActiveUnitIndex) : IEvent;
    
    public void Enter()
    {
        var availableUnits = _data.Active.TurnOrderIds.Count == 0
            ? _data.Setup.PlaceOrderIds
            : _data.Active.TurnOrderIds;
        
        _data.Active.TurnOrderIds.Set(availableUnits.Shuffle(Rng.Instance).ToList());
        _data.Active.ActiveUnitIndex = 0;
        _bus.Publish(new StartedEvent(_data.Active.TurnOrderIds, _data.Active.ActiveUnitIndex));
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}
