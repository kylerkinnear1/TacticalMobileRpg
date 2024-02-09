using Rpg.Mobile.GameSdk.Infrastructure;

namespace Rpg.Mobile.GameSdk;

public interface ISetupGame
{
    IScene Setup(IGame game);
}

public interface IScene
{
    void Update(TimeSpan delta);
    void Render();
}

public interface IGame
{
    ISceneBuilder<TScene> AddScene<TScene>() where TScene : IScene;
}

public interface ISceneBuilder<TScene>
{
    IStateBuilder<TState> AddState<TState>(TState state);
}

public interface IStateBuilder<TState>
{
    IStateBuilder<TState> AddRenderer<TRenderer>() where TRenderer : IRenderState<TState>;
    IStateBuilder<TState> UpdateWith<TUpdate>() where TUpdate : IUpdateState<TState>;
    IStateBuilder<TState> CalculateWith<TCalc>() where TCalc : ICalculateState<TState>;
    IStateBuilder<TState> AsUpdatedGameObject<TGameObject>() where TGameObject : IUpdateGameObject<TState>;
    IStateBuilder<TState> AsCalculatedGameObject<TGameObject>() where TGameObject : ICalculateGameObject<TState>;
    IStateBuilder<TState> CreateWith<TFactory>(Factory<TState> factory);
    IStateBuilder<TState> CreateWith<TArgs, TFactory>(Factory<TArgs, TState> factory);
}
