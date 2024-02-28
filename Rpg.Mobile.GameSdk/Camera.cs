using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface ICamera : IUpdateComponent,
    IDrawable // TODO: Remove
{
    PointF FocalPoint { get; set; }
    float Width { get; set; }
    float Height { get; set; }
}

public class Camera : ICamera
{
    private readonly List<IComponent> _components;

    public Camera(List<IComponent> components)
    {
        _components = components;
    }

    public PointF FocalPoint { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public ComponentBase? Target { get; set; }

    public void Update(TimeSpan delta)
    {
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.Translate(FocalPoint.X - (Width / 2), FocalPoint.Y - (Height) / 2);
        foreach (var node in _components.SelectMany(x => x.All))
        {
            canvas.SaveState();
            canvas.Translate(node.Bounds.X, node.Bounds.Y);
            node.Render(canvas, dirtyRect);
            canvas.RestoreState();
        }

        canvas.RestoreState();
    }
}
