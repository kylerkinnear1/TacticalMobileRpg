using System.Collections.Concurrent;

namespace Rpg.Mobile.GameSdk;

public class EventBus
{
    private readonly ConcurrentDictionary<Type, List<Action<object>>> _notificationHandlers = new();

    public void Publish<T>(T evnt)
    {
        if (!_notificationHandlers.TryGetValue(evnt.GetType(), out var handlers))
            return;

        handlers.ForEach(x => x(evnt));
    }

    public void Subscribe<T>(Action<T> handler)
    {
        var handlers = _notificationHandlers.GetOrAdd(typeof(T), _ => new());
        handlers.Add(x => handler((T)x));
    }
}
