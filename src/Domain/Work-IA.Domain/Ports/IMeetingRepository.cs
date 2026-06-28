using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Ports;

public interface IMeetingRepository
{
    Task AddAsync(Meeting meeting, CancellationToken ct = default);
    Task<List<Meeting>> GetAllAsync(CancellationToken ct = default);
}
