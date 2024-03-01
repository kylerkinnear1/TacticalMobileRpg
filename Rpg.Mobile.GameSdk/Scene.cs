using Microsoft.Maui;

namespace Rpg.Mobile.GameSdk;

public abstract class SceneBase
{
    public List<IComponent> ComponentTree { get; } = new();
    public List<(IHaveBounds Bounds, Action<TouchEvent> Handler)> TouchUpHandlers { get; } = new();
    public Camera ActiveCamera { get; set; }
    public IGraphicsView View { get; }

    private IEnumerable<IComponent> ComponentUpdates => ComponentTree;
    public IEnumerable<IUpdateComponent> Updates => 
        ComponentUpdates.SelectMany(x => x.All).Cast<IUpdateComponent>().Append(ActiveCamera);

    protected SceneBase(Camera? camera = null)
    {
        ActiveCamera = camera ?? new(ComponentTree);
    }

    protected T Add<T>(T component) where T : IComponent
    {
        ComponentTree.Add(component);
        return component;
    }
}
