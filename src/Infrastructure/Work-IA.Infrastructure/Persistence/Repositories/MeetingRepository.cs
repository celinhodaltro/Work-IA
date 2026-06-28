using Microsoft.EntityFrameworkCore;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Ports;

namespace Work_IA.Infrastructure.Persistence.Repositories;

public sealed class MeetingRepository : IMeetingRepository
{
    private readonly WorkIaDbContext _context;

    public MeetingRepository(WorkIaDbContext context) => _context = context;

    public async Task AddAsync(Meeting meeting, CancellationToken ct = default)
        => await _context.Meetings.AddAsync(meeting, ct);

    public async Task<List<Meeting>> GetAllAsync(CancellationToken ct = default)
        => await _context.Meetings.ToListAsync(ct);
}
