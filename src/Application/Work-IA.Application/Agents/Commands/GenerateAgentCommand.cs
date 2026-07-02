using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed record GenerateAgentCommand(string Name, string Title, AgentCareerLevel Level) : IRequest<AgentId>, IRequiresUnitOfWork;
