using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk;

namespace Rpg.Mobile.GameEngine.Render2d;

public class Renderer2d<TState> : IDrawable, IRenderState<TState>
{
    private readonly 

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        throw new NotImplementedException();
    }

    public void Render(TState state, ICanvas canvas, RectF dirtyRect)
    {
        throw new NotImplementedException();
    }
}
