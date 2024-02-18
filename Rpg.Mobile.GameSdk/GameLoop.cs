using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface IGameLoop
{
    void Start();

    void AddUpdate(IUpdateGameObject update);
    void AddRenderer(IRenderGameObject renders);
    void AddGameObject(IGameObject gameObject);

    void RemoveUpdate(IUpdateGameObject update);
    void RemoveRenderer(IRenderGameObject renders);
    void RemoveGameObject(IGameObject gameObject);

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
    private readonly List<IRenderGameObject> _renders = new();
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
    public void AddRenderer(IRenderGameObject renders) => _renders.Add(renders);
    public void AddGameObject(IGameObject gameObject)
    {
        AddUpdate(gameObject);
        AddRenderer(gameObject);
    }

    public void RemoveUpdate(IUpdateGameObject update) => _updates.Remove(update);
    public void RemoveRenderer(IRenderGameObject renders) => _renders.Remove(renders);

    public void RemoveGameObject(IGameObject gameObject)
    {
        _updates.Remove(gameObject);
    }

    public void AddTouchUpHandler(Action<TouchEvent> handler) => _touchUpHandlers.Add((handler, null));
    public void AddTouchUpHandler(Action handler) => _touchUpHandlers.Add((_ => handler(), null));
    public void AddTouchUpHandler(Action<TouchEvent> handler, Func<RectF> bounds) => _touchUpHandlers.Add((handler, bounds));
    public void AddTouchUpHandler(Action handler, Func<RectF> bounds) => _touchUpHandlers.Add((_ => handler(), bounds));

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        foreach (var render in _renders)
            render.Render(canvas, dirtyRect);
    }

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
