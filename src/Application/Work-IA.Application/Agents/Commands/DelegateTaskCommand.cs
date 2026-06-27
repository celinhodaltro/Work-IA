using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed record DelegateTaskCommand(
    string Title,
    string Description,
    AgentRole TargetRole,
    TaskPriority Priority) : IRequest<TaskResult>;
