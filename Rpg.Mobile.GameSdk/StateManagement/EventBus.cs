using System.Collections.Concurrent;

namespace Rpg.Mobile.GameSdk.StateManagement;

public interface IEvent { }

public interface IEventBus
{
    void Publish<T>(T evnt) where T : IEvent;
    ISubscription Subscribe<T>(Action<T> handler) where T : IEvent;
    void Unsubscribe<T>(Action<T> handler) where T : IEvent;

    Task PublishAsync<T>(T evnt) where T : IEvent;
    ISubscription SubscribeAsync<T>(Func<T, Task> handler) where T : IEvent;
    void UnsubscribeAsync<T>(Func<T, Task> handler) where T : IEvent;
}

public class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<ActionHandlerWrapper>> _actionNotificationHandlers = new();
    private readonly ConcurrentDictionary<Type, List<TaskHandlerWrapper>> _taskNotificationHandlers = new();

    public void Publish<T>(T evnt) where T : IEvent
    {
        if (_actionNotificationHandlers.TryGetValue(evnt.GetType(), out var actionHandlers))
        {
            List<ActionHandlerWrapper>? copiedActionHandlers;
            lock (actionHandlers)
            {
                copiedActionHandlers = actionHandlers.ToList();
            }
            
            copiedActionHandlers.ForEach(x => x.WrappedHandler(evnt));
        }

        if (_taskNotificationHandlers.TryGetValue(evnt.GetType(), out var taskHandlers))
        {
            List<TaskHandlerWrapper>? copiedTaskHandlers;
            lock (taskHandlers)
            {
                copiedTaskHandlers = taskHandlers.ToList();
            }

            copiedTaskHandlers.ForEach(x => x.WrappedHandler(evnt).GetAwaiter().GetResult());
        }
    }

    public ISubscription Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var handlers = _actionNotificationHandlers.GetOrAdd(typeof(T), _ => new());
        lock (handlers)
        {
            handlers.Add(new ActionHandlerWrapper<T>(handler));
        }
        
        return new Subscription(() => Unsubscribe(handler));
    }

    public void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        var handlers = _actionNotificationHandlers.GetOrAdd(typeof(T), _ => new());
        lock (handlers)
        {
            handlers.RemoveAll(x => ((ActionHandlerWrapper<T>)x).OriginalHandler == handler);
        }
    }

    public async Task PublishAsync<T>(T evnt) where T : IEvent
    {
        if (_actionNotificationHandlers.TryGetValue(evnt.GetType(), out var actionHandlers))
        {
            List<ActionHandlerWrapper>? copiedActionHandlers;
            lock (actionHandlers)
            {
                copiedActionHandlers = actionHandlers.ToList();
            }
            
            copiedActionHandlers.ForEach(x => x.WrappedHandler(evnt));
        }

        if (_taskNotificationHandlers.TryGetValue(evnt.GetType(), out var taskHandlers))
        {
            List<TaskHandlerWrapper>? copiedTaskHandlers;
            lock (taskHandlers)
            {
                copiedTaskHandlers = taskHandlers.ToList();
            }

            await Task.WhenAll(copiedTaskHandlers.Select(x => x.WrappedHandler(evnt)));
        }
    }

    public ISubscription SubscribeAsync<T>(Func<T, Task> handler) where T : IEvent
    {
        var tasks = _taskNotificationHandlers.GetOrAdd(typeof(T), _ => new());
        lock (tasks)
        {
            tasks.Add(new TaskHandlerWrapper<T>(handler));
        }

        return new Subscription(() => UnsubscribeAsync(handler));
    }

    public void UnsubscribeAsync<T>(Func<T, Task> handler) where T : IEvent
    {
        var handlers = _taskNotificationHandlers.GetOrAdd(typeof(T), _ => new());
        lock (handlers)
        {
            handlers.RemoveAll(x => ((TaskHandlerWrapper<T>)x).OriginalHandler == handler);
        }
    }
}

public interface ISubscription : IDisposable;
public class Subscription(Action _unsubscribe) : ISubscription
{
    public void Dispose() => _unsubscribe();
}

public record ActionHandlerWrapper(Action<object> WrappedHandler);
public record ActionHandlerWrapper<T>(Action<T> OriginalHandler) : ActionHandlerWrapper(x => OriginalHandler((T)x));

public record TaskHandlerWrapper(Func<object, Task> WrappedHandler);
public record TaskHandlerWrapper<T>(Func<T, Task> OriginalHandler) : TaskHandlerWrapper(x => OriginalHandler((T)x));
