using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.StateMachines.Phases.NewRound;

public class NewRoundPhase(Context _context) : IBattlePhase
{
    public void Enter()
    {
        _context.Data.TurnOrder.Set(_context.Data.TurnOrder.Shuffle(Rng.Instance).ToList());
        _context.Main.Units.Values.ToList().ForEach(x => x.HealthBar.HasGone = false);
        _context.Data.ActiveUnitIndex = 0;
        _context.Data.ActiveUnitStartPosition = _context.Data.UnitCoordinates[_context.Data.CurrentUnit];
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}
