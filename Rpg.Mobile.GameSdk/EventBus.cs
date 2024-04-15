using System.Collections.Concurrent;

namespace Rpg.Mobile.GameSdk;

public interface IEvent
{
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

    // TODO: Something less ugly
    public void AddSubscriptions(EventBus other)
    {
        foreach (var newHandler in other._notificationHandlers)
        {
            var existingHandlers = _notificationHandlers.GetOrAdd(newHandler.Key, _ => new());
            existingHandlers.AddRange(newHandler.Value);
        }
    }
}
