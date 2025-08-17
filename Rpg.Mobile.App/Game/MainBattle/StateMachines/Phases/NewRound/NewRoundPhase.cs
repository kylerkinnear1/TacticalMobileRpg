using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.NewRound;

public class NewRoundPhase(Context _context) : IBattlePhase
{
    public void Enter()
    {
        var availableUnits = _context.Data.TurnOrder.Count == 0
            ? _context.Data.PlaceOrder
            : _context.Data.TurnOrder;
        
        _context.Data.TurnOrder.Set(availableUnits.Shuffle(Rng.Instance).ToList());
        _context.Data.ActiveUnitIndex = 0;
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}
