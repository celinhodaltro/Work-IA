using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Queries;

public sealed record ListAgentsQuery : IRequest<IReadOnlyList<Agent>>;
