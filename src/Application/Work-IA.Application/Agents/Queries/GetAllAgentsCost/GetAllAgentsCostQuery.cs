using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Queries;

public sealed record GetAllAgentsCostQuery(int Days = 30) : IRequest<List<AgentCostSummary>>;
