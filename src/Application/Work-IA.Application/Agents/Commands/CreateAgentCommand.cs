using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed record CreateAgentCommand(
    string Name,
    string Title,
    Guid? RoleId = null,
    bool CanRead = true,
    bool CanWrite = true,
    bool CanDelegate = false,
    Guid? ReportsTo = null) : IRequest<AgentId>, IRequiresUnitOfWork;
