using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Ports;

public interface IBoardRepository
{
    Task<BoardTask?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<BoardTask>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(BoardTask task, CancellationToken ct = default);
    Task UpdateAsync(BoardTask task, CancellationToken ct = default);
}
