using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface IGameLoop
{
    void Start();
}

public interface IUpdateLoop
{
    void Update(TimeSpan delta);
}

public interface IRenderLoop
{
    void Render();
}

public class GameLoop : IGameLoop
{
    private readonly IDispatcher _dispatcher;
    private readonly IUpdateLoop _updateLoop;
    private readonly IRenderLoop _renderLoop;

    private DateTime _lastUpdate;

    private const int LoopTimeLimitMs = 16;

    public GameLoop(IDispatcher dispatcher, IUpdateLoop updateLoop, IRenderLoop renderLoop)
    {
        _dispatcher = dispatcher;
        _updateLoop = updateLoop;
        _renderLoop = renderLoop;
        _lastUpdate = DateTime.UtcNow;
    }

    public void Start()
    {
        var startTime = DateTime.UtcNow;
        var delta = startTime - _lastUpdate;

        _updateLoop.Update(delta);
        _lastUpdate = startTime;

        var postTime = DateTime.UtcNow;
        var updateDuration = LoopTimeLimitMs - (postTime - startTime).TotalMilliseconds;

        _renderLoop.Render();

        // TODO: Could add a safety check, but that would be slow for a performance critical area.
        // TODO: Any bit math that can be done with 16 to make the game just skip if it can't render in time instead of crash?
        var delayUntilNextUpdate = Math.Min(updateDuration, LoopTimeLimitMs);
        delayUntilNextUpdate = Math.Max(delayUntilNextUpdate, 0);
        _dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delayUntilNextUpdate), Start);
    }
}

public interface IGameLoopFactory
{
    GameLoop Create(
        GraphicsView view,
        IScene scene,
        IEnumerable<IUpdateGameObject> updates,
        IEnumerable<IRenderGameObject> renders);
}

public class GameLoopFactory : IGameLoopFactory
{
    public GameLoop Create(
        GraphicsView view,
        IScene scene,
        IEnumerable<IUpdateGameObject> updates,
        IEnumerable<IRenderGameObject> renders)
    {
        var update = new UpdateLoop(updates, scene);
        var render = new RenderLoop(view, scene, renders);
        var game = new GameLoop(view.Dispatcher, update, render);
        view.Drawable = render;
        return game;
    }
}

public class UpdateLoop : IUpdateLoop
{
    private readonly IEnumerable<IUpdateGameObject> _updates;
    private readonly IScene _scene;

    public UpdateLoop(IEnumerable<IUpdateGameObject> updates, IScene scene)
    {
        _updates = updates;
        _scene = scene;
    }

    public void Update(TimeSpan delta)
    {
        _scene.Update(delta);
        foreach (var update in _updates)
            update.Update(delta);
    }
}

public class RenderLoop : IRenderLoop, IDrawable
{
    private readonly IGraphicsView _view;
    private readonly IScene _scene;
    private readonly IEnumerable<IRenderGameObject> _renders;

    public RenderLoop(IGraphicsView view, IScene scene, IEnumerable<IRenderGameObject> renders)
    {
        _view = view;
        _scene = scene;
        _renders = renders;
    }

    public void Render() => _view.Invalidate();

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _scene.Render(canvas, dirtyRect);
        foreach (var render in _renders)
            render.Render(canvas, dirtyRect);
    }
}
