using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Queries;

public sealed record GetAgentQuery(AgentId Id) : IRequest<Agent?>;
