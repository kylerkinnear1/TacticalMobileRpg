namespace Rpg.Mobile.GameSdk.StateManagement;

public interface IState
{
    public void Enter() { }
    public void Execute(float deltaTime);
    public void Leave() { }
}

public interface IStateMachine
{
    void ChangeState(IState newState);
    void Update(float deltaTime);
}

public class StateMachine : IStateMachine
{
    private IState? _currentState;

    public void ChangeState(IState newState)
    {
        _currentState?.Leave();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Update(float deltaTime) => _currentState?.Execute(deltaTime);
}