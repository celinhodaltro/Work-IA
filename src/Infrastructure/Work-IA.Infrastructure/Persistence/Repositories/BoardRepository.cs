using Microsoft.EntityFrameworkCore;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Ports;

namespace Work_IA.Infrastructure.Persistence.Repositories;

public sealed class BoardRepository : IBoardRepository
{
    private readonly WorkIaDbContext _context;

    public BoardRepository(WorkIaDbContext context) => _context = context;

    public async Task<BoardTask?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.BoardTasks.FindAsync(new object[] { id }, ct);

    public async Task<List<BoardTask>> GetAllAsync(CancellationToken ct = default)
        => await _context.BoardTasks.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(BoardTask task, CancellationToken ct = default)
        => await _context.BoardTasks.AddAsync(task, ct);

    public Task UpdateAsync(BoardTask task, CancellationToken ct = default)
    {
        _context.BoardTasks.Update(task);
        return Task.CompletedTask;
    }
}
