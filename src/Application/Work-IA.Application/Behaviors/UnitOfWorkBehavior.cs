using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;

namespace Work_IA.Application.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork, IDomainEventDispatcher domainEventDispatcher)
    {
        _unitOfWork = unitOfWork;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IRequiresUnitOfWork)
            return await next();

        var response = await next();

        // 1. Coleta eventos pendentes registrados pelos repositórios
        var domainEvents = await _unitOfWork.GetAndClearDomainEventsAsync(cancellationToken);

        // 2. Despacha eventos (EventStore adiciona StoredEvent ao DbSet + MediatR publica)
        if (domainEvents.Count > 0)
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        // 3. Persiste TUDO (entidades + StoredEvents) em uma única transação
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
