using Microsoft.Extensions.DependencyInjection;

namespace Rpg.Mobile.GameSdk.Infrastructure;

public delegate TResult Factory<TResult>();
public delegate TResult Factory<TArgs, TResult>(TArgs args);

public static class FactoryExtensions
{
    public static IServiceCollection AddFactory<T>(this IServiceCollection services) where T : class, new() =>
        services.AddSingleton(_ => new T());

    public static IServiceCollection AddFactory<TArgs, TResult>(this IServiceCollection services, Factory<TArgs, TResult> factory) =>
        services.AddSingleton(factory);
}
