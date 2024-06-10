using System.Collections.Concurrent;

namespace Rpg.Mobile.GameSdk.StateManagement;

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
    private readonly ConcurrentDictionary<Type, List<ActionHandlerWrapper>> _notificationHandlers = new();

    public void Publish<T>(T evnt) where T : IEvent
    {
        if (!_notificationHandlers.TryGetValue(evnt.GetType(), out var handlers))
            return;

        var copiedHandlers = handlers.ToList();
        copiedHandlers.ForEach(x => x.WrappedHandler(evnt));
    }

    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var handlers = _notificationHandlers.GetOrAdd(typeof(T), _ => new());
        handlers.Add(new ActionHandlerWrapper<T>(handler));
    }

    public void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        var handlers = _notificationHandlers.GetOrAdd(typeof(T), _ => new());
        handlers.RemoveAll(x => ((ActionHandlerWrapper<T>)x).OriginalHandler == handler);
    }
}

public record ActionHandlerWrapper(Action<object> WrappedHandler);
public record ActionHandlerWrapper<T>(Action<T> OriginalHandler) : ActionHandlerWrapper(x => OriginalHandler((T)x));
