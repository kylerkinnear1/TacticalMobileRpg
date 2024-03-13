using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

namespace Rpg.Mobile.GameSdk;

public interface IGameLoop
{
    void Start();
}

public class GameLoop : IGameLoop
{
    private readonly SceneBase _scene;
    private readonly IGraphicsView _view;
    private readonly IDispatcher _dispatcher;

    public GameLoop(SceneBase scene, IGraphicsView view, IDispatcher dispatcher)
    {
        _scene = scene;
        _view = view;
        _dispatcher = dispatcher;
    }

    private DateTime _lastUpdate = DateTime.UtcNow;

    private const int LoopTimeLimitMs = 16;

    public void Start()
    {
        var startTime = DateTime.UtcNow;
        var delta = startTime - _lastUpdate;

        var deltaTime = (float)delta.TotalSeconds;
        foreach (var node in _scene.Updates)
            node.Update(deltaTime);

        _scene.Update(deltaTime);

        _lastUpdate = startTime;

        var postTime = DateTime.UtcNow;
        var updateDuration = LoopTimeLimitMs - (postTime - startTime).TotalMilliseconds;

        _view.Invalidate();

        var delayUntilNextUpdate = Math.Min(updateDuration, LoopTimeLimitMs);
        delayUntilNextUpdate = Math.Max(delayUntilNextUpdate, 0);
        _dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delayUntilNextUpdate), Start);
    }

    public void OnTouchUp(TouchEventArgs touch)
    {
        var touchedComponents = _scene.ComponentTree
            .SelectMany(x => x.All)
            .Select(x => new
            {
                Bounds = x.IgnoreCamera ? x.AbsoluteBounds : x.AbsoluteBounds.Offset(_scene.ActiveCamera.Offset),
                Component = x
            })
            .Where(x => x.Component.Visible && touch.Touches.Any(x.Bounds.Contains))
            .ToList();

        var topComponent = touchedComponents.LastOrDefault();
        topComponent?.Component.OnTouchUp(touch.Touches
            .Select(x => new PointF(x.X - topComponent.Bounds.X, x.Y - topComponent.Bounds.Y)));
    }
}

public interface IGameLoopFactory
{
    GameLoop Create(GraphicsView view, SceneBase scene);
}

public class GameLoopFactory : IGameLoopFactory
{
    public GameLoop Create(GraphicsView view, SceneBase scene)
    {
        var game = new GameLoop(scene, view, view.Dispatcher);
        view.Drawable = scene.ActiveCamera;
        view.EndInteraction += (_, e) => game.OnTouchUp(e);
        return game;
    }
}
