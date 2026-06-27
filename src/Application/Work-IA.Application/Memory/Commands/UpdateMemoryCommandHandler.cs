using MediatR;
using Work_IA.Application.Common.Exceptions;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Commands;

public sealed class UpdateMemoryCommandHandler : IRequestHandler<UpdateMemoryCommand>
{
    private readonly IMemoryStore _memoryStore;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMemoryCommandHandler(IMemoryStore memoryStore, IUnitOfWork unitOfWork)
    {
        _memoryStore = memoryStore;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateMemoryCommand request, CancellationToken cancellationToken)
    {
        var entry = await _memoryStore.GetByIdAsync(request.Id, cancellationToken);

        if (entry is null)
            throw new NotFoundException(nameof(MemoryEntry), request.Id.Value);

        if (request.NewScore.HasValue)
            entry.UpdateScore(request.NewScore.Value);

        if (request.TagsToAdd is not null)
        {
            foreach (var tag in request.TagsToAdd)
                entry.AddTag(tag);
        }

        await _memoryStore.SaveAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
