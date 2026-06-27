using Work_IA.Domain.Abstractions;

namespace Work_IA.Application.Common.Interfaces;

public interface IEventStore
{
    Task SaveAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IDomainEvent>> GetByAggregateIdAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IDomainEvent>> GetByTypeAsync(string eventType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IDomainEvent>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
