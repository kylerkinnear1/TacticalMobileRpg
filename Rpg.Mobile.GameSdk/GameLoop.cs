﻿using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;

namespace Rpg.Mobile.GameSdk;

public interface IGameLoop
{
    void Start();
}

public class GameLoop : IGameLoop
{
    private readonly IDispatcher _dispatcher;
    private readonly IScene _scene;

    private DateTime _lastUpdate;

    private const int LoopTimeLimitMs = 16;

    public GameLoop(IDispatcher dispatcher, IScene scene)
    {
        _dispatcher = dispatcher;
        _scene = scene;
        _lastUpdate = DateTime.UtcNow;
    }

    public void Start()
    {
        var startTime = DateTime.UtcNow;
        var delta = startTime - _lastUpdate;

        _scene.Update(delta);

        _lastUpdate = startTime;

        var postTime = DateTime.UtcNow;
        var updateDuration = LoopTimeLimitMs - (postTime - startTime).TotalMilliseconds;

        _scene.Render();

        // TODO: Could add a safety check, but that would be slow for a performance critical area.
        // TODO: Any bit math that can be done with 16 to make the game just skip if it can't render in time instead of crash?
        var delayUntilNextUpdate = Math.Min(updateDuration, LoopTimeLimitMs);
        delayUntilNextUpdate = Math.Max(delayUntilNextUpdate, 0);
        _dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(delayUntilNextUpdate), Start);
    }
}
