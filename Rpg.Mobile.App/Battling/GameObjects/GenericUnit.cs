using Rpg.Mobile.GameSdk;
using Rpg.Mobile.GameSdk.Extensions;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Rpg.Mobile.App.Battling.GameObjects;

public class GenericUnitState
{
    public PointF Location { get; set; }
    public IImage Sprite { get; set; }
    public float Scale { get; set; }

    public GenericUnitState(PointF location, IImage sprite, float scale)
    {
        Location = location;
        Sprite = sprite;
        Scale = scale;
    }
}

public class GenericUnitGameObject : IUpdateState<GenericUnitState>, IRenderState<GenericUnitState>
{
    public void Update(GenericUnitState state)
    {
        state.Location = state.Location.X > 300 ? new PointF(100f, 100f) : new PointF(state.Location.X + 1f, state.Location.Y + 1f);
    }

    public void Render(GenericUnitState state, ICanvas canvas, RectF dirtyRect)
    {
        canvas.Draw(state.Sprite, state.Location, state.Scale);
    }
}
