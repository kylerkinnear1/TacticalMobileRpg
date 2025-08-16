using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk.Core;

public class Camera(List<IComponent> _components) : IUpdateComponent, IDrawable
{
    public PointF Offset { get; set; }
    public ComponentBase? Target { get; set; }
    public SizeF Size { get; private set; } = SizeF.Zero; // TODO: This is a HACK SUPREME!

    public void Update(float deltaTime)
    {
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        Size = dirtyRect.Size;
        foreach (var node in _components.SelectMany(x => x.All).Where(x => x.Visible))
        {
            canvas.SaveState();
            var x = node.AbsoluteBounds.X + (node.IgnoreCamera ? 0f : Offset.X);
            var y = node.AbsoluteBounds.Y + (node.IgnoreCamera ? 0f : Offset.Y);

            canvas.Translate(x, y);
            node.Render(canvas, dirtyRect);
            canvas.RestoreState();
        }
    }
}
