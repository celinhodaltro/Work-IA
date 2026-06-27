using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Queries;

public sealed record GetAgentCostQuery(AgentId AgentId, int Days = 30) : IRequest<AgentCostSummary?>;
