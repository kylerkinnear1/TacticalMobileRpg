using System.Collections.Concurrent;

namespace Rpg.Mobile.GameSdk.Core;

public interface IEvent
{
}

// TODO: Something less embarrassing.
public static class Bus
{
    public static readonly EventBus Global = new();
}

public class EventBus
{
    private readonly ConcurrentDictionary<Type, List<Action<object>>> _notificationHandlers = new();

    public void Publish<T>(T evnt) where T : IEvent
    {
        if (!_notificationHandlers.TryGetValue(evnt.GetType(), out var handlers))
            return;

        handlers.ForEach(x => x(evnt));
    }

    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var handlers = _notificationHandlers.GetOrAdd(typeof(T), _ => new());
        handlers.Add(x => handler((T)x));
    }
}
