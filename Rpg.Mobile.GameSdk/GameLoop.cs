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

    private const int TickLimitMs = 16;

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
        var updateDuration = TickLimitMs - (postUpdate - currentUpdate).TotalMilliseconds;

        // Could add a safety check, but that would be slow for a performance critical area.
        var delayUntilNextUpdate = Math.Min(updateDuration, TickLimitMs);
        _dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delayUntilNextUpdate), Start);
    }
}
