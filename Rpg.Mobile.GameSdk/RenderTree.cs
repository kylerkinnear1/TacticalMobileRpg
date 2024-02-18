using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public record Node<T>(T Value, List<Node<T>> Children);

public record Renderer(IRenderGameObject GameObject, Func<CoordinateF> PositionProvider);

public class RenderTree
{
    private readonly IEnumerable<Node<Renderer>> _renderNodes;

    public RenderTree(IEnumerable<Node<Renderer>> renderNodes)
    {
        _renderNodes = renderNodes;
    }

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        var renderQueue = new Queue<Node<Renderer>>();
        var transformQueue = new Queue<CoordinateF?>();
        foreach (var node in _renderNodes)
        {
            renderQueue.Enqueue(node);
            transformQueue.Enqueue(CoordinateF.Zero);
        }
        
        while (renderQueue.TryDequeue(out var node) && transformQueue.TryDequeue(out var transform))
        {
            if (transform is not null)
                canvas.Translate(transform.X, transform.Y);

            node.Value.GameObject.Render(canvas, dirtyRect);
            if (node.Children.Count <= 0)
                continue;

            var position = node.Value.PositionProvider();
            for (var i = 0; i < node.Children.Count; i++)
            {
                renderQueue.Enqueue(node.Children[i]);
                var childTransform = i switch
                {
                    0 => position,
                    _ when i == node.Children.Count - 1 => -position,
                    _ => null
                };
                transformQueue.Enqueue(childTransform);
            }
        }
    }
}
