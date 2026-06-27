using Microsoft.EntityFrameworkCore;

namespace Work_IA.Infrastructure.Persistence;

public sealed class WorkIaDbContext : DbContext
{
    public WorkIaDbContext(DbContextOptions<WorkIaDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkIaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditableEntities()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                var createdAt = entry.Entity.GetType().GetProperty("CreatedAt");
                if (createdAt is not null && createdAt.CanWrite)
                {
                    createdAt.SetValue(entry.Entity, now);
                }
            }

            if (entry.State == EntityState.Modified)
            {
                var updatedAt = entry.Entity.GetType().GetProperty("UpdatedAt");
                if (updatedAt is not null && updatedAt.CanWrite)
                {
                    updatedAt.SetValue(entry.Entity, now);
                }
            }
        }
    }
}
