using Rpg.Mobile.App.Utils;
using Rpg.Mobile.GameSdk.StateManagement;
using Rpg.Mobile.GameSdk.Utilities;
using static Rpg.Mobile.App.Game.MainBattle.States.BattlePhaseMachine;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.NewRound;

public class NewRoundPhase : IBattlePhase
{
    public record StartedEvent : IEvent;

    private readonly Context _context;

    public NewRoundPhase(Context context) => _context = context;

    public void Enter()
    {
        _context.Data.TurnOrder.Set(_context.Data.TurnOrder.Shuffle(Rng.Instance).ToList());
        _context.Main.Units.Values.ToList().ForEach(x => x.HealthBar.HasGone = false);
        _context.Data.ActiveUnitIndex = 0;
        _context.Data.ActiveUnitStartPosition = _context.Data.UnitCoordinates[_context.Data.CurrentUnit];
    }

    public void Execute(float deltaTime)
    {
        // TODO: Maybe something better
        Bus.Global.Publish(new StartedEvent());
    }

    public void Leave()
    {
    }
}
