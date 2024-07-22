using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.App.Game.MainBattle.States.Phases.Active;

public class ActivePhase : IBattlePhase
{
    public interface IStep : IState { }

    private readonly StateMachine<IStep> _step;

    public ActivePhase(StateMachine<IStep> step) => _step = step;

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
