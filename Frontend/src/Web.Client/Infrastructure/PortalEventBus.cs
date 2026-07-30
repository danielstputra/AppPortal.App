namespace Web.Infrastructure;

public class PortalEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Publish<T>(T eventData) where T : class
    {
        if (_handlers.TryGetValue(typeof(T), out var handlers))
        {
            foreach (var handler in handlers.OfType<Action<T>>())
                handler(eventData);
        }
    }

    public IDisposable Subscribe<T>(Action<T> handler) where T : class
    {
        var type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Delegate>();

        _handlers[type].Add(handler);

        return new Subscription(() => { _handlers[type].Remove(handler); });
    }

    private record Subscription(Action Unsubscribe) : IDisposable
    {
        public void Dispose() => Unsubscribe();
    }
}

public record NotificationEvent(string Title, string Message, NotificationType Type);
public enum NotificationType { Info, Success, Warning, Error }
