using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Commands;

public sealed class RecordMemoryCommandHandler : IRequestHandler<RecordMemoryCommand, MemoryId>
{
    private readonly IMemoryStore _memoryStore;
    private readonly IUnitOfWork _unitOfWork;

    public RecordMemoryCommandHandler(IMemoryStore memoryStore, IUnitOfWork unitOfWork)
    {
        _memoryStore = memoryStore;
        _unitOfWork = unitOfWork;
    }

    public async Task<MemoryId> Handle(RecordMemoryCommand request, CancellationToken cancellationToken)
    {
        var entry = MemoryEntry.Create(
            request.Type,
            request.Title,
            request.Content,
            request.Tags,
            request.Score,
            request.AgentId);

        await _memoryStore.SaveAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entry.Id;
    }
}
