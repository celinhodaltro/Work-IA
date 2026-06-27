using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Commands;

public sealed record RecordMemoryCommand(
    MemoryType Type,
    string Title,
    string Content,
    List<string>? Tags,
    int Score,
    AgentId? AgentId) : IRequest<MemoryId>, IRequiresUnitOfWork;
