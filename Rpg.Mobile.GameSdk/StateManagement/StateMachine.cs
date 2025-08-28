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
    T? State { get; }

    void Change(T? newState);
}

public class StateMachine : StateMachine<IState>
{
    public StateMachine(IState? state) : base(state) { }
    public StateMachine() : base() { }
}

public class StateMachine<T>(T? state) : IStateMachine<T>
    where T : class, IState
{
    public T? State { get; private set; } = state;

    public StateMachine() : this(null) { }

    public void Change(T? newState)
    {
        State?.Leave();
        State = newState;
        State?.Enter();
    }

    public void Execute(float deltaTime) => State?.Execute(deltaTime);
}