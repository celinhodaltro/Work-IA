using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;

namespace Work_IA.Application.DomainEvents.Queries;

public sealed class GetDomainEventsQueryHandler : IRequestHandler<GetDomainEventsQuery, IReadOnlyList<IDomainEvent>>
{
    private readonly IEventStore _eventStore;

    public GetDomainEventsQueryHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<IReadOnlyList<IDomainEvent>> Handle(GetDomainEventsQuery request, CancellationToken cancellationToken)
    {
        return await _eventStore.GetByAggregateIdAsync(request.AggregateId, cancellationToken);
    }
}
