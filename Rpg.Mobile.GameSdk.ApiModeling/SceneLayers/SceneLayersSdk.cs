using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.ApiModeling.Core;

namespace Rpg.Mobile.GameSdk.ApiModeling.SceneLayers;

public interface ILayerBuilder
{
    IImage? Background { get; set; }
    int Depth { get; set; }

    IEntityBuilder<TEntity> AddEntity<TEntity>(Func<IServiceProvider, TEntity> initialize);
    IEntityBuilder<TEntity> AddEntity<TEntity>(TEntity state);

    void SetBackground(IImage image);
    void SetDepth(int z);

    // TODO: Music
    // TODO: Events
}
