using System.Collections.Concurrent;

namespace Rpg.Mobile.GameSdk.StateManagement;

public interface IEvent { }

public interface IEventBus
{
    void Publish<T>(T evnt) where T : IEvent;
    ISubscription Subscribe<T>(Action<T> handler) where T : IEvent;
    void Unsubscribe<T>(Action<T> handler) where T : IEvent;
}

public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<ActionHandlerWrapper>> _notificationHandlers = new();

    public void Publish<T>(T evnt) where T : IEvent
    {
        if (!_notificationHandlers.TryGetValue(evnt.GetType(), out var handlers))
            return;

        var copiedHandlers = handlers.ToList();
        copiedHandlers.ForEach(x => x.WrappedHandler(evnt));
    }

    public ISubscription Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var handlers = _notificationHandlers.GetOrAdd(typeof(T), _ => new());
        handlers.Add(new ActionHandlerWrapper<T>(handler));
        return new Subscription(() => Unsubscribe(handler));
    }

    public void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        var handlers = _notificationHandlers.GetOrAdd(typeof(T), _ => new());
        handlers.RemoveAll(x => ((ActionHandlerWrapper<T>)x).OriginalHandler == handler);
    }
}

public interface ISubscription : IDisposable;
public class Subscription(Action _unsubscribe) : ISubscription
{
    public void Dispose() => _unsubscribe();
}

public record ActionHandlerWrapper(Action<object> WrappedHandler);
public record ActionHandlerWrapper<T>(Action<T> OriginalHandler) : ActionHandlerWrapper(x => OriginalHandler((T)x));
