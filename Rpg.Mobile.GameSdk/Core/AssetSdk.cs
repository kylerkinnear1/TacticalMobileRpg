namespace Rpg.Mobile.GameSdk.Core;

public interface IAssetBuilder
{
    IAssetBuilder<TAsset> Load<TAsset>();
}

public interface IAssetBuilder<TAsset>
{
    IGameBuilder UnloadOn<TEvent>();
    IGameBuilder UnloadOn<TEvent>(Func<TEvent, bool> filter);
    IGameBuilder UnloadOn<TEvent>(Func<TEvent, TAsset, bool> filter);
}
