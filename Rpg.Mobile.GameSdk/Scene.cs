namespace Rpg.Mobile.GameSdk;

public abstract class SceneBase
{
    public List<IComponent> ComponentTree { get; } = new();
    public List<(IHaveBounds Bounds, Action<TouchEvent> Handler)> TouchUpHandlers { get; } = new();
    public ICamera ActiveCamera { get; protected set; }

    private readonly List<ICamera> _cameras = new();
    public IEnumerable<ICamera> Cameras => _cameras;

    private IEnumerable<IUpdateComponent> ComponentUpdates => ComponentTree;
    public IEnumerable<IUpdateComponent> Updates => ComponentUpdates.Concat(_cameras);

    protected SceneBase()
    {
        var camera = new Camera(ComponentTree);
        _cameras.Add(camera);
        ActiveCamera = camera;
    }

    protected void Add(IComponent component) => ComponentTree.Add(component);
    protected void AddTouchUpHandler(IHaveBounds component, Action<TouchEvent> handler) => TouchUpHandlers.Add((component, handler));
}
