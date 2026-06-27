using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Queries;

public sealed class SearchMemoriesQueryHandler : IRequestHandler<SearchMemoriesQuery, IReadOnlyList<MemoryEntry>>
{
    private readonly IMemoryStore _memoryStore;

    public SearchMemoriesQueryHandler(IMemoryStore memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public async Task<IReadOnlyList<MemoryEntry>> Handle(SearchMemoriesQuery request, CancellationToken cancellationToken)
    {
        return await _memoryStore.SearchAsync(request.Query, request.Limit, cancellationToken);
    }
}
