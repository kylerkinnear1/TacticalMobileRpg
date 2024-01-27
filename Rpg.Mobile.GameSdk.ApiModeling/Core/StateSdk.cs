namespace Rpg.Mobile.GameSdk.ApiModeling.Core;

public interface IMutator<TEntity>
{
    void Mutate(TEntity state, TimeSpan deltaTime);
}

public interface ICalculator<TEntity>
{
    TEntity Calculate(TEntity state, TimeSpan deltaTime);
}

public interface IEntityBuilder
{
    IEntityBuilder AddEntity<TEntity>(Action<IEntityBuilder<TEntity>> configure);
}

public interface IEntityBuilder<TEntity>
{
    IEntityBuilder<TEntity> AddMutator<TUpdateState>() where TUpdateState : IMutator<TEntity>;
    IEntityBuilder<TEntity> AddCalculator<TCalculateState>() where TCalculateState : ICalculator<TEntity>;


    // TODO: Add colliders
    // TODO: Add sound
    // TODO: Add event subscriber (before update loop)
}
