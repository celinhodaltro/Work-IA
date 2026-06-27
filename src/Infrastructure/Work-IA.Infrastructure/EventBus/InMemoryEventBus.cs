using System.Collections.Concurrent;
using Work_IA.Application.Common.Interfaces;

namespace Work_IA.Infrastructure.EventBus;

public sealed class InMemoryEventBus : IEventBus, IDisposable
{
    private readonly ConcurrentDictionary<string, List<Func<IIntegrationEvent, CancellationToken, Task>>> _handlers = new();
    private readonly ConcurrentDictionary<string, Func<IIntegrationEvent, CancellationToken, Task>> _subscriptions = new();
    private readonly ConcurrentQueue<DeadLetterEntry> _deadLetterQueue = new();
    private bool _disposed;

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IIntegrationEvent
    {
        var eventType = typeof(T).Name;

        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            var snapshot = handlers.ToArray();
            _ = Task.WhenAll(snapshot.Select(handler => SafeExecute(handler, @event, cancellationToken)));
        }

        return Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(string subscriberId, Func<T, CancellationToken, Task> handler) where T : IIntegrationEvent
    {
        var eventType = typeof(T).Name;
        var key = $"{eventType}_{subscriberId}";

        Func<IIntegrationEvent, CancellationToken, Task> wrapper = async (e, ct) =>
        {
            if (e is T typedEvent)
            {
                await handler(typedEvent, ct);
            }
        };

        _subscriptions[key] = wrapper;

        _handlers.AddOrUpdate(
            eventType,
            _ => [wrapper],
            (_, list) =>
            {
                list.Add(wrapper);
                return list;
            });

        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync<T>(string subscriberId) where T : IIntegrationEvent
    {
        var eventType = typeof(T).Name;
        var key = $"{eventType}_{subscriberId}";

        if (_subscriptions.TryRemove(key, out var wrapper))
        {
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(wrapper);
                if (handlers.Count == 0)
                {
                    _handlers.TryRemove(eventType, out _);
                }
            }
        }

        return Task.CompletedTask;
    }

    private async Task SafeExecute<T>(Func<IIntegrationEvent, CancellationToken, Task> handler, T @event, CancellationToken cancellationToken) where T : IIntegrationEvent
    {
        try
        {
            await handler(@event, cancellationToken);
        }
        catch (Exception ex)
        {
            _deadLetterQueue.Enqueue(new DeadLetterEntry(@event, ex));
        }
    }

    public IReadOnlyCollection<DeadLetterEntry> GetDeadLetterEvents()
    {
        return _deadLetterQueue.ToArray();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _handlers.Clear();
        _subscriptions.Clear();
        _disposed = true;
    }
}

public sealed record DeadLetterEntry(IIntegrationEvent Event, Exception Exception);
