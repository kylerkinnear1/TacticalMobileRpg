using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.States.BattlePhaseStateMachine;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.NewRound;

public class NewRoundPhase : IBattlePhase
{
    public record NewRoundStartedEvent : IEvent;

    private readonly Context _context;

    public NewRoundPhase(Context context) => _context = context;

    public void Enter()
    {
        _context.Data.TurnOrder.Set(_context.Data.TurnOrder.Shuffle(Rng.Instance).ToList());
        _context.Main.Units.Values.ToList().ForEach(x => x.HealthBar.HasGone = false);
        _context.Data.ActiveUnitIndex = 0;
        _context.Data.ActiveUnitStartPosition = _context.Data.UnitCoordinates[_context.Data.CurrentUnit];
        Bus.Global.Publish(new NewRoundStartedEvent());
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}
