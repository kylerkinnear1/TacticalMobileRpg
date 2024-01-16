using Rpg.Mobile.GameSdk.SceneLayers;

namespace Rpg.Mobile.GameSdk.Core;

public interface ISceneBuilder
{
    ISceneBuilder AddScene<TScene>(Action<ISceneBuilder<TScene>> configure);
}

public interface ISceneBuilder<TScene>
{
    ISceneBuilder<TScene> ConfigureBackground(Action<ILayerBuilder> builder);
    ISceneBuilder<TScene> AddLayer(Action<ILayerBuilder> builder);
    ISceneBuilder<TScene> UnloadOn<TEvent>();
    ISceneBuilder<TScene> UnloadOn<TEvent>(Func<TScene, TEvent, bool> filter);
}