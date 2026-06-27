using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Queries;

public sealed class GetMemoriesByTagsQueryHandler : IRequestHandler<GetMemoriesByTagsQuery, IReadOnlyList<MemoryEntry>>
{
    private readonly IMemoryStore _memoryStore;

    public GetMemoriesByTagsQueryHandler(IMemoryStore memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public async Task<IReadOnlyList<MemoryEntry>> Handle(GetMemoriesByTagsQuery request, CancellationToken cancellationToken)
    {
        return await _memoryStore.GetByTagsAsync(request.Tags, request.Limit, cancellationToken);
    }
}
