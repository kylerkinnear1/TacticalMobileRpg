using System.Collections.Concurrent;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Rpg.Mobile.GameSdk.Inputs;
using Rpg.Mobile.GameSdk.StateManagement;

namespace Rpg.Mobile.GameSdk.Core;

public interface IGameLoop
{
    void Start();
    void Execute(Action actionToExecute);
    void ChangeScene(SceneBase scene);
}

public class GameLoop(
    SceneBase _scene, 
    GraphicsView _view, 
    IDispatcher _dispatcher,
    IMouse _mouse)
    : IGameLoop
{
    private DateTime _lastUpdate = DateTime.UtcNow;
    private readonly ConcurrentQueue<Action> _gameThreadActionQueue = new();

    private const int LoopTimeLimitMs = 16;

    public void Start()
    {
        var startTime = DateTime.UtcNow;
        var delta = startTime - _lastUpdate;
        var deltaTime = (float)delta.TotalSeconds;
        
        ProcessActionQueue();
        
        HandleInput();
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

    private void ProcessActionQueue()
    {
        while (_gameThreadActionQueue.TryDequeue(out var action))
            action();
    }

    public void Execute(Action actionToExecute) =>
        _gameThreadActionQueue.Enqueue(actionToExecute);

    public void ChangeScene(SceneBase scene) => Execute(() =>
    {
        _scene.OnExit();
        _scene = scene;
        _view.Drawable = scene.ActiveCamera;
        _scene.OnEnter();
    });

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
        var cameraOffset = _scene.ActiveCamera.Offset;

        foreach (var rootComponent in _scene.ComponentTree)
        {
            foreach (var component in rootComponent.All)
            {
                if (!component.Visible)
                    continue;

                var absoluteBounds = component.AbsoluteBounds;
                var bounds = component.IgnoreCamera 
                    ? absoluteBounds 
                    : absoluteBounds.Offset(cameraOffset);

                if (bounds.Contains(mousePosition.X, mousePosition.Y))
                {
                    var relativeX = mousePosition.X - bounds.X;
                    var relativeY = mousePosition.Y - bounds.Y;
                    component.OnHover(new PointF(relativeX, relativeY));
                }
            }
        }
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
