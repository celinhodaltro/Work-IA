using Microsoft.EntityFrameworkCore;
using Work_IA.Application.Services;
using Work_IA.Infrastructure.Persistence;

namespace Work_IA.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private readonly WorkIaDbContext _context;

    public AuditService(WorkIaDbContext context)
    {
        _context = context;
    }

    public void Record(string userId, string action, string resourceType, string resourceId, string details)
    {
        _context.Set<AuditEntry>().Add(new AuditEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Details = details,
            Timestamp = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    public IReadOnlyList<AuditEntry> GetRecent(int count = 50)
    {
        return _context.Set<AuditEntry>()
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToList();
    }
}
