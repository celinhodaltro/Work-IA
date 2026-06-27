using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;

namespace Work_IA.Infrastructure.Services;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly IEventStore _eventStore;
    private readonly IEventBus _eventBus;

    public DomainEventDispatcher(IMediator mediator, IEventStore eventStore, IEventBus eventBus)
    {
        _mediator = mediator;
        _eventStore = eventStore;
        _eventBus = eventBus;
    }

    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var @event in events)
        {
            await _eventStore.SaveAsync(@event, cancellationToken);

            await _mediator.Publish(@event, cancellationToken);

            if (@event is IIntegrationEvent integrationEvent)
            {
                await _eventBus.PublishAsync(integrationEvent, cancellationToken);
            }
        }
    }
}
