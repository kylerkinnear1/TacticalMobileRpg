using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.Cleanup;

public class CleanupPhase : IBattlePhase
{
    public record RoundEnded : IEvent;

    public void Enter()
    {
    }

    public void Execute(float deltaTime)
    {
    }

    public void Leave()
    {
    }
}
