using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface IUpdateComponent
{
    void Update(float deltaTime);
}

public interface IHaveBounds
{
    RectF Bounds { get; }
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

    bool IgnoreCamera { get; }
    RectF AbsoluteBounds { get; }
    PointF Position { get; set; }

    // TODO: Remove the set
    IComponent? Parent { get; set; }
    IEnumerable<IComponent> Parents { get; }

    void OnHover(PointF touches);
    void OnTouchUp(IEnumerable<PointF> touches);
    IComponent RemoveChild(IComponent child);
    void SetParent(IComponent? parent);
}

public abstract class ComponentBase : IComponent
{
    public IComponent? Parent { get; set; }
    public RectF Bounds { get; protected set; }
    public PointF Position
    {
        get => Bounds.Location;
        set => Bounds = new(value, Bounds.Size);
    }

    public bool IgnoreCamera { get; set; } = false;

    public IEnumerable<IComponent> Parents
    {
        get
        {
            var currentParent = Parent;
            while (currentParent != null)
            {
                yield return currentParent;
                currentParent = currentParent.Parent;
            }
        }
    }

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
            var x = Bounds.X;
            var y = Bounds.Y;
            foreach (var parent in Parents)
            {
                x += parent.Bounds.X;
                y += parent.Bounds.Y;
            }

            return new(x, y, Bounds.Width, Bounds.Height);
        }
    }

    public virtual void OnTouchUp(IEnumerable<PointF> touches) { }
    public virtual void OnHover(PointF hover) { }

    public abstract void Update(float deltaTime);
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

    public T AddChild<T>(T child) where T : IComponent
    {
        child.Parent = this;
        ChildList.Add(child);
        return child;
    }

    public IComponent RemoveChild(IComponent child)
    {
        if (ChildList.Remove(child))
            child.Parent = null;

        return child;
    }

    public void SetParent(IComponent? parent)
    {
        parent?.RemoveChild(this);
        Parent = parent;
    }
}
