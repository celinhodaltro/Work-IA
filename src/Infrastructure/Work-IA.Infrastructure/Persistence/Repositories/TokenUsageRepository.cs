using Microsoft.EntityFrameworkCore;
using Work_IA.Domain.Agents;

namespace Work_IA.Infrastructure.Persistence.Repositories;

public sealed class TokenUsageRepository : ITokenUsageRepository
{
    private readonly WorkIaDbContext _context;

    public TokenUsageRepository(WorkIaDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TokenUsage usage, CancellationToken ct = default)
    {
        var entity = new TokenUsageEntity
        {
            Id = Guid.NewGuid(),
            AgentId = usage.AgentId.Value,
            ModelName = usage.ModelName,
            TokensIn = usage.TokensIn,
            TokensOut = usage.TokensOut,
            Cost = usage.Cost,
            TaskDescription = usage.TaskDescription,
            Timestamp = usage.Timestamp
        };
        await _context.Set<TokenUsageEntity>().AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<TokenUsage>> GetByAgentAsync(AgentId agentId, int days = 30, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _context.Set<TokenUsageEntity>()
            .Where(t => t.AgentId == agentId.Value && t.Timestamp >= cutoff)
            .OrderByDescending(t => t.Timestamp)
            .Select(t => MapToDomain(t))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TokenUsage>> GetAllAsync(int days = 30, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        return await _context.Set<TokenUsageEntity>()
            .Where(t => t.Timestamp >= cutoff)
            .OrderByDescending(t => t.Timestamp)
            .Select(t => MapToDomain(t))
            .ToListAsync(ct);
    }

    public async Task<AgentCostSummary?> GetAgentSummaryAsync(AgentId agentId, int days = 30, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var query = _context.Set<TokenUsageEntity>().Where(t => t.AgentId == agentId.Value && t.Timestamp >= cutoff);

        var summary = await query
            .GroupBy(t => 1)
            .Select(g => new AgentCostSummary
            {
                AgentId = agentId,
                TotalTokensIn = g.Sum(t => t.TokensIn),
                TotalTokensOut = g.Sum(t => t.TokensOut),
                TotalCost = (decimal)g.Sum(t => (double)t.Cost),
                TaskCount = g.Count(),
                MostUsedModel = g.GroupBy(t => t.ModelName).OrderByDescending(x => x.Count()).Select(x => x.Key).FirstOrDefault() ?? ""
            }).FirstOrDefaultAsync(ct) ?? new AgentCostSummary { AgentId = agentId };

        var agent = await _context.Set<AgentEntity>().FirstOrDefaultAsync(a => a.Id == agentId.Value, ct);
        if (agent is not null)
        {
            summary.AgentName = agent.Name;
            summary.AgentTitle = agent.Title ?? "";
        }

        return summary;
    }

    public async Task<List<AgentCostSummary>> GetAllAgentsSummaryAsync(int days = 30, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var summaries = await _context.Set<TokenUsageEntity>()
            .Where(t => t.Timestamp >= cutoff)
            .GroupBy(t => t.AgentId)
            .Select(g => new AgentCostSummary
            {
                AgentId = new AgentId(g.Key),
                TotalTokensIn = g.Sum(t => t.TokensIn),
                TotalTokensOut = g.Sum(t => t.TokensOut),
                TotalCost = (decimal)g.Sum(t => (double)t.Cost),
                TaskCount = g.Count(),
                MostUsedModel = g.GroupBy(t => t.ModelName).OrderByDescending(x => x.Count()).Select(x => x.Key).FirstOrDefault() ?? ""
            }).ToListAsync(ct);

        var agents = await _context.Set<AgentEntity>().ToListAsync(ct);
        foreach (var s in summaries)
        {
            var a = agents.FirstOrDefault(x => x.Id == s.AgentId.Value);
            if (a is not null)
            {
                s.AgentName = a.Name;
                s.AgentTitle = a.Title ?? "";
            }
        }
        return summaries;
    }

    private static TokenUsage MapToDomain(TokenUsageEntity e)
    {
        return TokenUsage.Record(new AgentId(e.AgentId), e.ModelName, e.TokensIn, e.TokensOut, e.TaskDescription);
    }
}
