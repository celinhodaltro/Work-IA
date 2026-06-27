using MediatR;
using Work_IA.Domain.Abstractions;

namespace Work_IA.Application.DomainEvents.Queries;

public sealed record GetDomainEventsByTypeQuery(string EventType) : IRequest<IReadOnlyList<IDomainEvent>>;
