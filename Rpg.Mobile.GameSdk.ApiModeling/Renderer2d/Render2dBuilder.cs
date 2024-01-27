using Rpg.Mobile.GameSdk.ApiModeling.Core;

namespace Rpg.Mobile.GameSdk.ApiModeling.Renderer2d;

public static class Renderer2dExtensions
{
    public static IEntityBuilder<TEntity> AddRenderer2d<TEntity>(this IEntityBuilder<TEntity> builder, Action<IRender2dBuilder<TEntity>> configure)
    {
        throw new NotImplementedException();
        configure(default);
    }
}

public interface IRender2dBuilder<TEntity>
{
    IRender2dBuilder<TEntity> AddRenderer<TRenderer>() where TRenderer : IRenderer2d<TEntity>;
}
