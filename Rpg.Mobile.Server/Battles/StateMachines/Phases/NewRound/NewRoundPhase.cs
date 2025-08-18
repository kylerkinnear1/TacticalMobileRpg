using Rpg.Mobile.Api;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.NewRound;

public class NewRoundPhase(Context _context) : IBattlePhase
{
    public void Enter()
    {
        var availableUnits = _context.Data.Active.TurnOrder.Count == 0
            ? _context.Data.Setup.PlaceOrder
            : _context.Data.Active.TurnOrder;
        
        _context.Data.Active.TurnOrder.Set(availableUnits.Shuffle(Rng.Instance).ToList());
        _context.Data.Active.ActiveUnitIndex = 0;
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}
