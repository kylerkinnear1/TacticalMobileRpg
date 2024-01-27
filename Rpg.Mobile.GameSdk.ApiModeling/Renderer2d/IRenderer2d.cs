using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk.ApiModeling.Renderer2d;

public interface IRenderer2d
{
    void Render(ICanvas canvas, RectF bounds);
}

public interface IRenderer2d<TEntity>
{
    void Render(TEntity entity, ICanvas canvas, RectF bounds);
}
