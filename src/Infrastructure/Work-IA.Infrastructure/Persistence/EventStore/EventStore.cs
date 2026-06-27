using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;

namespace Work_IA.Infrastructure.Persistence.EventStore;

public sealed class EventStore : IEventStore
{
    private readonly WorkIaDbContext _context;
    private readonly IEventTypeResolver _typeResolver;
    private readonly JsonSerializerOptions _jsonOptions;

    public EventStore(WorkIaDbContext context, IEventTypeResolver typeResolver)
    {
        _context = context;
        _typeResolver = typeResolver;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task SaveAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var storedEvent = new StoredEvent
        {
            EventId = @event.EventId,
            EventType = @event.GetType().FullName!,
            AggregateType = @event.GetType().Assembly.GetName().Name ?? "Unknown",
            Data = JsonSerializer.Serialize(@event, @event.GetType(), _jsonOptions),
            OccurredOn = @event.OccurredOn
        };

        await _context.StoredEvents.AddAsync(storedEvent, cancellationToken);
    }

    public async Task<IReadOnlyList<IDomainEvent>> GetByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        var storedEvents = await _context.StoredEvents
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.OccurredOn)
            .ToListAsync(cancellationToken);

        return DeserializeEvents(storedEvents);
    }

    public async Task<IReadOnlyList<IDomainEvent>> GetByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        var storedEvents = await _context.StoredEvents
            .Where(e => e.EventType == eventType)
            .OrderByDescending(e => e.OccurredOn)
            .ToListAsync(cancellationToken);

        return DeserializeEvents(storedEvents);
    }

    public async Task<IReadOnlyList<IDomainEvent>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var storedEvents = await _context.StoredEvents
            .Where(e => e.OccurredOn >= from && e.OccurredOn <= to)
            .OrderByDescending(e => e.OccurredOn)
            .ToListAsync(cancellationToken);

        return DeserializeEvents(storedEvents);
    }

    private IReadOnlyList<IDomainEvent> DeserializeEvents(List<StoredEvent> storedEvents)
    {
        var events = new List<IDomainEvent>();

        foreach (var stored in storedEvents)
        {
            var eventType = _typeResolver.ResolveType(stored.EventType);
            if (eventType is not null)
            {
                var deserialized = JsonSerializer.Deserialize(stored.Data, eventType, _jsonOptions);
                if (deserialized is IDomainEvent domainEvent)
                {
                    events.Add(domainEvent);
                }
            }
        }

        return events.AsReadOnly();
    }
}
