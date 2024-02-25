using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

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

        foreach (var node in _scene.Updates)
            node.Update(delta);

        _lastUpdate = startTime;

        var postTime = DateTime.UtcNow;
        var updateDuration = LoopTimeLimitMs - (postTime - startTime).TotalMilliseconds;

        _view.Invalidate();

        var delayUntilNextUpdate = Math.Min(updateDuration, LoopTimeLimitMs);
        delayUntilNextUpdate = Math.Max(delayUntilNextUpdate, 0);
        _dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delayUntilNextUpdate), Start);
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
        view.EndInteraction += (_, e) => throw new NotImplementedException();
        return game;
    }
}
