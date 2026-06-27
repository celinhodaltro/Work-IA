using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Memory;

namespace Work_IA.Application.Memory.Queries;

public sealed class GetMemoriesByTypeQueryHandler : IRequestHandler<GetMemoriesByTypeQuery, IReadOnlyList<MemoryEntry>>
{
    private readonly IMemoryStore _memoryStore;

    public GetMemoriesByTypeQueryHandler(IMemoryStore memoryStore)
    {
        _memoryStore = memoryStore;
    }

    public async Task<IReadOnlyList<MemoryEntry>> Handle(GetMemoriesByTypeQuery request, CancellationToken cancellationToken)
    {
        return await _memoryStore.GetByTypeAsync(request.Type, request.Limit, cancellationToken);
    }
}
