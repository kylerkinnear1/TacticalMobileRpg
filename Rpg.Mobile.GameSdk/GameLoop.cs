using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface IGameLoop
{
    IRenderTree RenderTree { get; }

    void Start();

    void AddUpdate(IUpdateGameObject update);
    void RemoveUpdate(IUpdateGameObject update);
    void AddGameObject(IGameObject gameObject);
    void AddGameObject(Node<IGameObject> nestedGameObject);
    void AddGameObject(IGameObject parent, params IGameObject[] children);

    void AddTouchUpHandler(Action<TouchEvent> handler);
    void AddTouchUpHandler(Action handler);
    void AddTouchUpHandler(Action<TouchEvent> handler, Func<RectF> bounds);
    void AddTouchUpHandler(Action handler, Func<RectF> bounds);
}

public interface IUpdateLoop
{
    void Update(TimeSpan delta);
}

public interface IRenderLoop
{
    void Render();
}

public class GameLoop : IGameLoop, IDrawable
{
    private readonly List<IUpdateGameObject> _updates = new();
    private readonly List<(Action<TouchEvent> Handler, Func<RectF>? BoundsProvider)> _touchUpHandlers = new();
    private readonly List<Node<Renderer>> _renders = new();
    private readonly IDispatcher _dispatcher;
    private readonly IGraphicsView _view;

    private DateTime _lastUpdate = DateTime.UtcNow;

    private const int LoopTimeLimitMs = 16;

    public GameLoop(
        IDispatcher dispatcher,
        IGraphicsView view)
    {
        _dispatcher = dispatcher;
        _view = view;
    }

    public IRenderTree RenderTree { get; } = new RenderTree();

    public void Start()
    {
        var startTime = DateTime.UtcNow;
        var delta = startTime - _lastUpdate;

        foreach (var update in _updates)
            update.Update(delta);

        _lastUpdate = startTime;

        var postTime = DateTime.UtcNow;
        var updateDuration = LoopTimeLimitMs - (postTime - startTime).TotalMilliseconds;

        _view.Invalidate();

        var delayUntilNextUpdate = Math.Min(updateDuration, LoopTimeLimitMs);
        delayUntilNextUpdate = Math.Max(delayUntilNextUpdate, 0);
        _dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delayUntilNextUpdate), Start);
    }

    public void AddUpdate(IUpdateGameObject update) => _updates.Add(update);
    public void RemoveUpdate(IUpdateGameObject update) => _updates.Remove(update);

    public void AddGameObject(IGameObject gameObject)
    {
        _updates.Add(gameObject);
        _renders.Add(new(new(gameObject)));
    }

    public void AddGameObject(Node<IGameObject> nestedGameObject)
    {
        throw new NotImplementedException();
    }

    public void AddGameObject(IGameObject parent, params IGameObject[] children)
    {
        throw new NotImplementedException();
    }

    public void AddTouchUpHandler(Action<TouchEvent> handler) => _touchUpHandlers.Add((handler, null));
    public void AddTouchUpHandler(Action handler) => _touchUpHandlers.Add((_ => handler(), null));
    public void AddTouchUpHandler(Action<TouchEvent> handler, Func<RectF> bounds) => _touchUpHandlers.Add((handler, bounds));
    public void AddTouchUpHandler(Action handler, Func<RectF> bounds) => _touchUpHandlers.Add((_ => handler(), bounds));

    public void Draw(ICanvas canvas, RectF dirtyRect) => RenderTree.Render(canvas, dirtyRect);

    public void OnTouchUp(TouchEvent touches)
    {
        if (touches.Touches.Length == 0)
            return;

        foreach (var handler in _touchUpHandlers.Where(x => 
                     x.BoundsProvider is null || touches.Touches.Any(y => x.BoundsProvider().Contains(y))))
        {
            handler.Handler(touches);
        }
    }
}

public interface IGameLoopFactory
{
    GameLoop Create(GraphicsView view);
}

public class GameLoopFactory : IGameLoopFactory
{
    public GameLoop Create(GraphicsView view)
    {
        var game = new GameLoop(view.Dispatcher, view);
        view.Drawable = game;
        view.EndInteraction += (_, e) => game.OnTouchUp(new(e.Touches));
        return game;
    }
}
