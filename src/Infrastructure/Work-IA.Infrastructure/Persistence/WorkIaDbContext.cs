using Microsoft.EntityFrameworkCore;
using Work_IA.Application.Services;

namespace Work_IA.Infrastructure.Persistence;

public sealed class WorkIaDbContext : DbContext
{
    public DbSet<AgentEntity> Agents => Set<AgentEntity>();
    public DbSet<StoredEvent> StoredEvents => Set<StoredEvent>();
    public DbSet<MemoryEntryEntity> MemoryEntries => Set<MemoryEntryEntity>();
    public DbSet<WorkflowInstanceEntity> WorkflowInstances => Set<WorkflowInstanceEntity>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public WorkIaDbContext(DbContextOptions<WorkIaDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkIaDbContext).Assembly);

        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.ToTable("AuditEntries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.Action).HasMaxLength(128);
            entity.Property(e => e.ResourceType).HasMaxLength(128);
            entity.Property(e => e.ResourceId).HasMaxLength(256);
            entity.Property(e => e.Details).HasMaxLength(2048);
        });

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
