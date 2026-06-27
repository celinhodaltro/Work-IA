using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed record CreateAgentCommand(string Name, AgentRole Role) : IRequest<AgentId>, IRequiresUnitOfWork;
