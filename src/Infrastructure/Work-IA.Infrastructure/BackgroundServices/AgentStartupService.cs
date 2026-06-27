using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Agents;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Infrastructure.BackgroundServices;

public sealed class AgentStartupService : BackgroundService, IAgentInitializationService
{
    private readonly AgentRegistry _registry;
    private readonly AgentRoleFactory _factory;
    private readonly ILogger<AgentStartupService> _logger;

    public AgentStartupService(AgentRegistry registry, AgentRoleFactory factory, ILogger<AgentStartupService> logger)
    {
        _registry = registry;
        _factory = factory;
        _logger = logger;
    }

    public async Task InitializeAllAgentsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting AI Office OS agent workforce...");

        var roles = Enum.GetValues<AgentRole>();

        foreach (var role in roles)
        {
            var agent = _factory.Create(role);
            _registry.Register(agent);
            await agent.InitializeAsync(cancellationToken);
            _logger.LogInformation("Agent {Role} initialized and running", role);
        }

        _logger.LogInformation("AI Office OS fully operational with {Count} agents", roles.Length);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeAllAgentsAsync(stoppingToken);
    }
}
