using Rpg.Mobile.GameSdk.Inputs;

namespace Rpg.Mobile.GameSdk.Core;

public abstract class SceneBase
{
    public List<IComponent> ComponentTree { get; } = new();
    public List<(IHaveBounds Bounds, Action<TouchEvent> Handler)> TouchUpHandlers { get; } = new();
    public Camera ActiveCamera { get; set; }

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

    public abstract void Update(float deltaTime);
}
