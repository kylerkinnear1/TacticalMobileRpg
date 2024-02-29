using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public class Camera : IUpdateComponent, IDrawable
{
    private readonly List<IComponent> _components;
    private readonly IGraphicsView _view;

    public Camera(List<IComponent> components, IGraphicsView view)
    {
        _components = components;
        _view = view;
    }

    public PointF Offset { get; set; }
    public ComponentBase? Target { get; set; }

    public void Update(TimeSpan delta)
    {
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.SaveState();
        canvas.Translate(
            Offset.X - (dirtyRect.Width / 2) + (Target?.Bounds.X ?? 0f), 
            Offset.Y - (dirtyRect.Height / 2) + (Target?.Bounds.Y ?? 0f));

        foreach (var node in _components.SelectMany(x => x.All))
        {
            canvas.SaveState();
            if (node.Parent is not null)
            {
                var x = node.Bounds.X;
                var y = node.Bounds.Y;
                foreach (var parent in node.Parents)
                {
                    x += parent.Bounds.X;
                    y += parent.Bounds.Y;
                }
                canvas.Translate(x, y);
            }

            node.Render(canvas, dirtyRect);
            canvas.RestoreState();
        }

        canvas.RestoreState();
    }
}
