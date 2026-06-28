using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Abstractions;

public interface IRoleRepository
{
    Task<RoleDefinition?> GetByIdAsync(RoleId id, CancellationToken ct = default);
    Task<List<RoleDefinition>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(RoleDefinition role, CancellationToken ct = default);
    Task UpdateAsync(RoleDefinition role, CancellationToken ct = default);
}

