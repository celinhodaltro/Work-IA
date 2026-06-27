using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Infrastructure.Persistence.Repositories;

public sealed class AgentRepository : IAgentRepository
{
    private readonly WorkIaDbContext _context;

    public AgentRepository(WorkIaDbContext context)
    {
        _context = context;
    }

    public async Task<Agent?> GetByIdAsync(AgentId id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<AgentEntity>()
            .FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken);

        return entity is null ? null : MapToDomain(entity);
    }

    public async Task AddAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(agent);
        await _context.Set<AgentEntity>().AddAsync(entity, cancellationToken);
    }

    public Task UpdateAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(agent);
        _context.Set<AgentEntity>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(agent);
        _context.Set<AgentEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Agent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Set<AgentEntity>()
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Agent>> GetByRoleAsync(AgentRole role, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Set<AgentEntity>()
            .Where(e => e.Role == (int)role)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Agent>> GetOnlineAsync(CancellationToken cancellationToken = default)
    {
        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
        var entities = await _context.Set<AgentEntity>()
            .Where(e => e.Status == (int)AgentStatus.Running
                && e.LastHeartbeat != null
                && e.LastHeartbeat >= fiveMinutesAgo)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    private static Agent MapToDomain(AgentEntity entity)
    {
        var ctor = typeof(Agent).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(c => c.GetParameters().Length == 3);

        var agent = (Agent)ctor.Invoke([
            AgentId.From(entity.Id),
            new AgentName(entity.Name),
            (AgentRole)entity.Role]);

        var agentType = typeof(Agent);
        agentType.GetProperty(nameof(Agent.Status))!.SetValue(agent, (AgentStatus)entity.Status);
        agentType.GetProperty(nameof(Agent.LastHeartbeat))!.SetValue(agent, entity.LastHeartbeat);

        var baseType = typeof(Entity<AgentId>);
        baseType.GetProperty(nameof(Entity<AgentId>.CreatedAt))!.SetValue(agent, entity.CreatedAt);
        baseType.GetProperty(nameof(Entity<AgentId>.UpdatedAt))!.SetValue(agent, entity.UpdatedAt);

        return agent;
    }

    private static AgentEntity MapToEntity(Agent agent)
    {
        return new AgentEntity
        {
            Id = agent.Id.Value,
            Name = agent.Name.Value,
            Role = (int)agent.Role,
            Status = (int)agent.Status,
            LastHeartbeat = agent.LastHeartbeat,
            CreatedAt = agent.CreatedAt,
            UpdatedAt = agent.UpdatedAt
        };
    }
}
