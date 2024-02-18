using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public record Node<T>(T Value, List<Node<T>> Children)
{
    public Node(T value) : this(value, new()) { }
}

public interface IRenderTree
{
    void Render(ICanvas canvas, RectF dirtyRect);

    void Add(IRenderGameObject render);
    void Add(IRenderGameObject render, IRenderGameObject child0, params IRenderGameObject[] children);
    void Add(Node<IRenderGameObject> nestedRenders);
    void AddToParent(IRenderGameObject parent, params IRenderGameObject[] children);
    void Remove(IRenderGameObject render);
}

public class RenderTree : IRenderTree
{
    private readonly List<Node<IRenderGameObject>> _renderers = new();

    public void Render(ICanvas canvas, RectF dirtyRect)
    {
        var renderQueue = new Queue<Node<IRenderGameObject>>();
        var transformQueue = new Queue<CoordinateF?>();
        foreach (var node in _renderers)
        {
            renderQueue.Enqueue(node);
            transformQueue.Enqueue(null);
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

    public void Add(Renderer render) => _renderers.Add(new(render));

    public void Add(Node<Renderer> nestedRenders)
    {
        throw new NotImplementedException();
    }

    public void AddToParent(IRenderGameObject parent, params Renderer[] children)
    {
        throw new NotImplementedException();
    }

    public void Remove(IRenderGameObject render)
    {
        throw new NotImplementedException();
    }
}
