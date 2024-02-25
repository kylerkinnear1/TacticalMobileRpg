using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface IUpdateComponent
{
    void Update(TimeSpan delta);
}

public interface IHaveBounds
{
    RectF Bounds { get; }
    RectF AbsoluteBounds { get; }
}

public interface IRenderComponent : IHaveBounds
{
    void Render(ICanvas canvas, RectF dirtyRect);
}

public interface IComponent : IUpdateComponent, IRenderComponent
{
    IEnumerable<IComponent> All { get; }
    IReadOnlyCollection<IComponent> Children { get; }
    IEnumerable<IComponent> Descendents { get; }

    // TODO: Remove the set
    IComponent? Parent { get; set; }

    void AddChild(IComponent child);
    void RemoveChild(IComponent child);
    void SetParent(IComponent? parent);
}

public abstract class ComponentBase : IComponent
{
    public IComponent? Parent { get; set; }
    
    protected List<IComponent> ChildList = new();
    public IReadOnlyCollection<IComponent> Children => ChildList;

    protected ComponentBase(RectF bounds)
    {
        Bounds = bounds;
    }

    public RectF AbsoluteBounds
    {
        get
        {
            var currentPosition = Bounds.Location;
            var parent = Parent;
            while (parent != null)
            {
                currentPosition = new(currentPosition.X + parent.Bounds.X, currentPosition.Y + parent.Bounds.Y);
                parent = parent.Parent;
            }

            return new(currentPosition.X, currentPosition.Y, Bounds.Width, Bounds.Height);
        }
    }

    public RectF Bounds { get; protected set; }

    public abstract void Update(TimeSpan delta);
    public abstract void Render(ICanvas canvas, RectF dirtyRect);

    public IEnumerable<IComponent> All => GetAllRecursive();
    public IEnumerable<IComponent> Descendents => GetAllRecursive().Skip(1);

    private IEnumerable<IComponent> GetAllRecursive()
    {
        var queue = new Queue<IComponent>();
        queue.Enqueue(this);

        while (queue.TryDequeue(out var node))
        {
            yield return node;
            foreach (var child in node.Children)
                queue.Enqueue(child);
        }
    }

    public void AddChild(IComponent child)
    {
        child.Parent = this;
        ChildList.Add(child);
    }

    public void RemoveChild(IComponent child)
    {
        if (ChildList.Remove(child))
            child.Parent = null;
    }

    public void SetParent(IComponent? parent)
    {
        parent?.RemoveChild(this);
        Parent = parent;
    }
}