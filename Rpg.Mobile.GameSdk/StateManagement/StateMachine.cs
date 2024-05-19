namespace Rpg.Mobile.GameSdk.StateManagement;

public interface IState
{
    public void Enter();
    public void Execute(float deltaTime);
    public void Leave();
}

public interface IStateMachine
{
    void Change(IState newState);
    void Execute(float deltaTime);
}

public class StateMachine : IStateMachine
{
    private IState? _currentState;

    public void Change(IState newState)
    {
        _currentState?.Leave();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Execute(float deltaTime) => _currentState?.Execute(deltaTime);
}