using Work_IA.Domain.Agents;

namespace Work_IA.Application.Common.Interfaces;

public interface IAgentInitializationService
{
    Task InitializeAllAgentsAsync(CancellationToken cancellationToken = default);
}
