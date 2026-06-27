using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Agents;

public interface ITokenUsageRepository
{
    Task AddAsync(TokenUsage usage, CancellationToken ct = default);
    Task<IReadOnlyList<TokenUsage>> GetByAgentAsync(AgentId agentId, int days = 30, CancellationToken ct = default);
    Task<IReadOnlyList<TokenUsage>> GetAllAsync(int days = 30, CancellationToken ct = default);
    Task<AgentCostSummary?> GetAgentSummaryAsync(AgentId agentId, int days = 30, CancellationToken ct = default);
    Task<List<AgentCostSummary>> GetAllAgentsSummaryAsync(int days = 30, CancellationToken ct = default);
}
