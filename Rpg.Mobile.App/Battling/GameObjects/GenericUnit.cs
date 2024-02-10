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

public class GenericUnitGameObject : IUpdateGameObject, IRenderGameObject
{
    private readonly GenericUnitState _state;

    public GenericUnitGameObject(GenericUnitState state) => _state = state;

    public void Update(TimeSpan delta)
    {
        _state.Location = _state.Location.X > 300f
            ? new(100f, 100f)
            : new(_state.Location.X + 1, _state.Location.Y + 1);
    }

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Draw(_state.Sprite, _state.Location, _state.Scale);
    }
}
