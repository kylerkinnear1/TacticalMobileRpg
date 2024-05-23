namespace Rpg.Mobile.GameSdk.StateManagement;

public interface IState
{
    public void Enter();
    public void Execute(float deltaTime);
    public void Leave();
}

public interface IStateMachine : IStateMachine<IState>
{
    void Execute(float deltaTime);
}

public interface IStateMachine<T> where T : class, IState
{
    void Change(T newState);
}

public class StateMachine<T> : IStateMachine<T> where T : class, IState
{
    private T? _currentState;

    public void Change(T newState)
    {
        _currentState?.Leave();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Execute(float deltaTime) => _currentState?.Execute(deltaTime);
}