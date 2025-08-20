using Rpg.Mobile.Api;
using Rpg.Mobile.Api.Battles.Data;
using Rpg.Mobile.GameSdk.Utilities;

namespace Rpg.Mobile.Server.Battles.StateMachines.Phases.NewRound;

public class NewRoundPhase(
    BattleData _data) : IBattlePhase
{
    public void Enter()
    {
        var availableUnits = _data.Active.TurnOrder.Count == 0
            ? _data.Setup.PlaceOrder
            : _data.Active.TurnOrder;
        
        _data.Active.TurnOrder.Set(availableUnits.Shuffle(Rng.Instance).ToList());
        _data.Active.ActiveUnitIndex = 0;
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}
