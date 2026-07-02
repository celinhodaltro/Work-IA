using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Agents;

namespace Work_IA.Infrastructure.Persistence.Repositories;

public sealed class AgentRepository : IAgentRepository
{
    private readonly WorkIaDbContext _context;
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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

        if (agent.DomainEvents.Count > 0)
        {
            _context.EnqueueDomainEvents(agent.DomainEvents);
            agent.ClearDomainEvents();
        }
    }

    public Task UpdateAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(agent);
        _context.Set<AgentEntity>().Update(entity);

        if (agent.DomainEvents.Count > 0)
        {
            _context.EnqueueDomainEvents(agent.DomainEvents);
            agent.ClearDomainEvents();
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Agent agent, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(agent);
        _context.Set<AgentEntity>().Remove(entity);

        if (agent.DomainEvents.Count > 0)
        {
            _context.EnqueueDomainEvents(agent.DomainEvents);
            agent.ClearDomainEvents();
        }

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Agent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Set<AgentEntity>()
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Agent>> GetByCareerLevelAsync(AgentCareerLevel level, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Set<AgentEntity>()
            .Where(e => e.CareerLevel == (int)level)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<IReadOnlyList<Agent>> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Set<AgentEntity>()
            .Where(e => e.Title == title)
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
        var agent = new Agent(
            AgentId.From(entity.Id),
            new AgentName(entity.Name),
            new AgentTitle(entity.Title),
            (AgentCareerLevel)entity.CareerLevel);

        var agentType = typeof(Agent);
        agentType.GetProperty(nameof(Agent.Status))!.SetValue(agent, (AgentStatus)entity.Status);
        agentType.GetProperty(nameof(Agent.ExperiencePoints))!.SetValue(agent, entity.ExperiencePoints);
        agentType.GetProperty(nameof(Agent.LastHeartbeat))!.SetValue(agent, entity.LastHeartbeat);
        agentType.GetProperty(nameof(Agent.JoinedAt))!.SetValue(agent, entity.JoinedAt);
        agentType.GetProperty(nameof(Agent.LastPromotionAt))!.SetValue(agent, entity.LastPromotionAt);
        agentType.GetProperty(nameof(Agent.IsHead))!.SetValue(agent, entity.IsHead);
        agentType.GetProperty(nameof(Agent.Permissions))!.SetValue(agent, new AgentPermissions(entity.CanRead, entity.CanWrite, entity.CanDelegate, entity.ReportsTo.HasValue ? AgentId.From(entity.ReportsTo.Value) : null));

        if (entity.MentorId.HasValue)
        {
            agentType.GetProperty(nameof(Agent.MentorId))!.SetValue(agent, AgentId.From(entity.MentorId.Value));
        }

        if (entity.RoleId.HasValue)
        {
            agentType.GetProperty(nameof(Agent.RoleId))!.SetValue(agent, RoleId.From(entity.RoleId.Value));
        }

        if (!string.IsNullOrEmpty(entity.SkillsJson))
        {
            var skills = JsonSerializer.Deserialize<List<SkillDto>>(entity.SkillsJson, _jsonOptions);
            if (skills is not null)
            {
                foreach (var skill in skills)
                {
                    agent.AssignSkill(new AgentSkill(skill.Name, skill.Proficiency));
                }
            }
        }

        var baseType = typeof(Entity<AgentId>);
        baseType.GetProperty(nameof(Entity<AgentId>.CreatedAt))!.SetValue(agent, entity.CreatedAt);
        baseType.GetProperty(nameof(Entity<AgentId>.UpdatedAt))!.SetValue(agent, entity.UpdatedAt);

        return agent;
    }

    private static AgentEntity MapToEntity(Agent agent)
    {
        return new AgentEntity
        {
            Id = agent.AgentId.Value,
            Name = agent.Name.Value,
            Title = agent.Title.Value,
            CareerLevel = (int)agent.CareerLevel,
            Status = (int)agent.Status,
            ExperiencePoints = agent.ExperiencePoints,
            SkillsJson = agent.Skills.Count > 0
                ? JsonSerializer.Serialize(agent.Skills.Select(s => new SkillDto { Name = s.Name, Proficiency = s.Proficiency }), _jsonOptions)
                : null,
            MentorId = agent.MentorId?.Value,
            RoleId = agent.RoleId?.Value,
            JoinedAt = agent.JoinedAt,
            LastPromotionAt = agent.LastPromotionAt,
            LastHeartbeat = agent.LastHeartbeat,
            IsHead = agent.IsHead,
            CanRead = agent.Permissions.CanRead,
            CanWrite = agent.Permissions.CanWrite,
            CanDelegate = agent.Permissions.CanDelegate,
            ReportsTo = agent.Permissions.ReportsTo?.Value,
            CreatedAt = agent.CreatedAt,
            UpdatedAt = agent.UpdatedAt
        };
    }

    private sealed class SkillDto
    {
        public string Name { get; set; } = string.Empty;
        public int Proficiency { get; set; }
    }
}
