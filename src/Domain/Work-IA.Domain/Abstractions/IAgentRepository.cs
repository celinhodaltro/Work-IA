using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Abstractions;

public interface IAgentRepository : IRepository<Agent, AgentId>
{
    Task<IReadOnlyList<Agent>> GetByRoleAsync(AgentRole role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Agent>> GetOnlineAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Agent>> GetAllAsync(CancellationToken cancellationToken = default);
}
