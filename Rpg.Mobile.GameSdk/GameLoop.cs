using Microsoft.Maui;
using Microsoft.Maui.Dispatching;

namespace Rpg.Mobile.GameSdk;

public interface IGameLoop
{
    void Start();
}

public class GameLoop : IGameLoop
{
    private readonly IDispatcher _dispatcher;
    private readonly IGraphicsView _view;

    private const int LoopTimeLimitMs = 16;

    public GameLoop(IDispatcher dispatcher, IGraphicsView view)
    {
        _dispatcher = dispatcher;
        _view = view;
    }

    public void Start()
    {
        var currentUpdate = DateTime.UtcNow;

        _view.Invalidate();

        var postUpdate = DateTime.UtcNow;
        var updateDuration = LoopTimeLimitMs - (postUpdate - currentUpdate).TotalMilliseconds;

        // TODO: Could add a safety check, but that would be slow for a performance critical area.
        // TODO: Any bit math that can be done with 16 to make the game just skip if it can't render in time instead of crash?
        var delayUntilNextUpdate = Math.Min(updateDuration, LoopTimeLimitMs);
        _dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delayUntilNextUpdate), Start);
    }
}
