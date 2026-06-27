using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Commands;

public sealed record UpdateMemoryCommand(
    MemoryId Id,
    int? NewScore,
    List<string>? TagsToAdd) : IRequest, IRequiresUnitOfWork;
