using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;

namespace Work_IA.Application.DomainEvents.Queries;

public sealed class GetDomainEventsByTypeQueryHandler : IRequestHandler<GetDomainEventsByTypeQuery, IReadOnlyList<IDomainEvent>>
{
    private readonly IEventStore _eventStore;

    public GetDomainEventsByTypeQueryHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<IReadOnlyList<IDomainEvent>> Handle(GetDomainEventsByTypeQuery request, CancellationToken cancellationToken)
    {
        return await _eventStore.GetByTypeAsync(request.EventType, cancellationToken);
    }
}
