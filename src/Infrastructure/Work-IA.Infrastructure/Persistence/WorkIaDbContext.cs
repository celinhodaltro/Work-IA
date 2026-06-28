using Microsoft.EntityFrameworkCore;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Application.Services;
using Work_IA.Domain.Abstractions; using Work_IA.Domain.Agents;

namespace Work_IA.Infrastructure.Persistence;

public sealed class WorkIaDbContext : DbContext, IUnitOfWork
{
    private readonly List<IDomainEvent> _pendingEvents = [];

    public DbSet<AgentEntity> Agents => Set<AgentEntity>();
    public DbSet<StoredEvent> StoredEvents => Set<StoredEvent>();
    public DbSet<MemoryEntryEntity> MemoryEntries => Set<MemoryEntryEntity>();
    public DbSet<WorkflowInstanceEntity> WorkflowInstances => Set<WorkflowInstanceEntity>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<BoardTask> BoardTasks => Set<BoardTask>(); public DbSet<TokenUsageEntity> TokenUsages => Set<TokenUsageEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();

    public WorkIaDbContext(DbContextOptions<WorkIaDbContext> options) : base(options) { }

    /// <summary>
    /// Registra um evento de domÃ­nio para ser persistido como StoredEvent e posteriormente despachado.
    /// </summary>
    internal void EnqueueDomainEvent(IDomainEvent domainEvent)
    {
        _pendingEvents.Add(domainEvent);
    }

    /// <summary>
    /// Registra mÃºltiplos eventos de domÃ­nio de uma vez.
    /// </summary>
    internal void EnqueueDomainEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        _pendingEvents.AddRange(domainEvents);
    }

    /// <summary>
    /// IUnitOfWork: Retorna os eventos pendentes e limpa a lista interna.
    /// Chamado pelo UnitOfWorkBehavior ANTES de SaveChangesAsync e DispatchAsync.
    /// O DomainEventDispatcher.DispatchAsync adiciona StoredEvent ao DbSet via EventStore,
    /// e o SaveChangesAsync posterior persiste tudo (entidades + StoredEvents) em uma transaÃ§Ã£o.
    /// </summary>
    Task<IReadOnlyList<IDomainEvent>> IUnitOfWork.GetAndClearDomainEventsAsync(CancellationToken cancellationToken)
    {
        var events = _pendingEvents.ToList().AsReadOnly();
        _pendingEvents.Clear();
        return Task.FromResult<IReadOnlyList<IDomainEvent>>(events);
    }

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
