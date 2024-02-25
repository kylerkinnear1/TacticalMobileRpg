using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface ICamera : IUpdateComponent,
    IDrawable // TODO: Remove
{
    PointF Position { get; }
    float Width { get; set; }
    float Height { get; set; }
}

public class Camera : ICamera, IDrawable
{
    private readonly List<IComponent> _components;

    public Camera(List<IComponent> components)
    {
        _components = components;
    }

    public PointF Position { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public ComponentBase? Target { get; set; }

    public void Update(TimeSpan delta)
    {
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var transforms = new Queue<PointF>();
        transforms.Enqueue(Target?.AbsoluteBounds.Center ?? new(Position.X + (Width / 2), Position.Y + (Height / 2)));
        foreach (var node in _components.SelectMany(x => x.All))
        {
            var transform = transforms.Dequeue();
            canvas.Translate(transform.X, transform.Y);

            node.Render(canvas, dirtyRect);
            if (node.Children.Count <= 0)
                continue;

            var position = node.Bounds.Location;
            for (var i = 0; i < node.Children.Count; i++)
            {
                var childTransform = i switch
                {
                    0 => position,
                    _ when i == node.Children.Count - 1 => new(-position.X, -position.Y),
                    _ => PointF.Zero
                };
                transforms.Enqueue(childTransform);
            }
        }
    }
}
