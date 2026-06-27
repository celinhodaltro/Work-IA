using Work_IA.Domain.Abstractions;

namespace Work_IA.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IDomainEvent>> GetAndClearDomainEventsAsync(CancellationToken cancellationToken = default);
}
