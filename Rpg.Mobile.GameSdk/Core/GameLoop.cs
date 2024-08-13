using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Inputs;

namespace Rpg.Mobile.GameSdk.Core;

public interface IGameLoop
{
    void Start();
}

public class GameLoop(SceneBase _scene, IGraphicsView _view, IDispatcher _dispatcher, IMouse _mouse)
    : IGameLoop
{
    private DateTime _lastUpdate = DateTime.UtcNow;

    private const int LoopTimeLimitMs = 16;

    public void Start()
    {
        HandleInput();

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

        foreach (var component in touchedComponents)
        {
            component.Component.OnTouchUp(touch.Touches
                .Select(x => new PointF(x.X - component.Bounds.X, x.Y - component.Bounds.Y)));
        }
    }

    private void HandleInput()
    {
        HandleHover();
    }

    private void HandleHover()
    {
        var mousePosition = _mouse.GetRelativeClientPosition();
        var hoveredComponents = _scene.ComponentTree
            .SelectMany(x => x.All)
            .Select(x => new
            {
                Bounds = x.IgnoreCamera ? x.AbsoluteBounds : x.AbsoluteBounds.Offset(_scene.ActiveCamera.Offset),
                Component = x
            })
            .Where(x => x.Component.Visible && x.Bounds.Contains(mousePosition.X, mousePosition.Y))
            .ToList();

        foreach (var component in hoveredComponents)
            component.Component.OnHover(new(mousePosition.X - component.Bounds.X, mousePosition.Y - component.Bounds.Y));
    }
}

public interface IGameLoopFactory
{
    GameLoop Create(GraphicsView view, SceneBase scene, IMouse mouse);
}

public class GameLoopFactory : IGameLoopFactory
{
    public GameLoop Create(GraphicsView view, SceneBase scene, IMouse mouse)
    {
        var game = new GameLoop(scene, view, view.Dispatcher, mouse);
        view.Drawable = scene.ActiveCamera;
        view.EndInteraction += (_, e) => game.OnTouchUp(e);
        return game;
    }
}
