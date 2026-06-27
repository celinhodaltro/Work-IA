using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Workflows;

namespace Work_IA.Infrastructure.Persistence.Repositories;

public sealed class WorkflowRepository : IWorkflowRepository
{
    private readonly WorkIaDbContext _context;

    public WorkflowRepository(WorkIaDbContext context)
    {
        _context = context;
    }

    public async Task<WorkflowInstance?> GetByIdAsync(WorkflowId id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<WorkflowInstanceEntity>()
            .FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task AddAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(instance);
        await _context.Set<WorkflowInstanceEntity>().AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(instance);
        _context.Set<WorkflowInstanceEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(instance);
        _context.Set<WorkflowInstanceEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<WorkflowInstance>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Set<WorkflowInstanceEntity>()
            .Where(e => e.State == (int)WorkflowInstanceState.Running
                || e.State == (int)WorkflowInstanceState.Pending)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<WorkflowInstance>> GetByStatusAsync(WorkflowInstanceState status, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Set<WorkflowInstanceEntity>()
            .Where(e => e.State == (int)status)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    private static WorkflowInstance MapToDomain(WorkflowInstanceEntity entity)
    {
        var ctor = typeof(WorkflowInstance).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(c => c.GetParameters().Length == 2
                && c.GetParameters()[0].ParameterType == typeof(string));

        var instance = (WorkflowInstance)ctor.Invoke([entity.WorkflowName, entity.TriggerEventId]);

        var baseType = typeof(Entity<WorkflowId>);
        baseType.GetProperty(nameof(Entity<WorkflowId>.Id))!.SetValue(instance, WorkflowId.From(entity.Id));
        baseType.GetProperty(nameof(Entity<WorkflowId>.CreatedAt))!.SetValue(instance, entity.CreatedAt);
        baseType.GetProperty(nameof(Entity<WorkflowId>.UpdatedAt))!.SetValue(instance, entity.UpdatedAt);

        var instanceType = typeof(WorkflowInstance);
        instanceType.GetProperty(nameof(WorkflowInstance.State))!.SetValue(instance, (WorkflowInstanceState)entity.State);
        instanceType.GetProperty(nameof(WorkflowInstance.CurrentStepIndex))!.SetValue(instance, entity.CurrentStepIndex);
        instanceType.GetProperty(nameof(WorkflowInstance.StartedAt))!.SetValue(instance, entity.StartedAt);
        instanceType.GetProperty(nameof(WorkflowInstance.CompletedAt))!.SetValue(instance, entity.CompletedAt);
        instanceType.GetProperty(nameof(WorkflowInstance.Error))!.SetValue(instance, entity.Error);

        return instance;
    }

    private static WorkflowInstanceEntity MapToEntity(WorkflowInstance instance)
    {
        return new WorkflowInstanceEntity
        {
            Id = instance.Id.Value,
            WorkflowName = instance.WorkflowName,
            TriggerEventId = instance.TriggerEventId,
            State = (int)instance.State,
            CurrentStepIndex = instance.CurrentStepIndex,
            StartedAt = instance.StartedAt,
            CompletedAt = instance.CompletedAt,
            Error = instance.Error,
            CreatedAt = instance.CreatedAt,
            UpdatedAt = instance.UpdatedAt
        };
    }
}
