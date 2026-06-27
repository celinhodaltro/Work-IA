using Work_IA.Domain.Memory;

namespace Work_IA.Application.Common.Interfaces;

public interface IMemoryStore
{
    Task SaveAsync(MemoryEntry entry, CancellationToken cancellationToken = default);
    Task<MemoryEntry?> GetByIdAsync(MemoryId memoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemoryEntry>> SearchAsync(string query, int limit = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemoryEntry>> GetByTypeAsync(MemoryType type, int limit = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemoryEntry>> GetByTagsAsync(string[] tags, int limit = 10, CancellationToken cancellationToken = default);
    Task DeleteAsync(MemoryId memoryId, CancellationToken cancellationToken = default);
}
