using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Agents;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Infrastructure.BackgroundServices;

public sealed class AgentStartupService : BackgroundService, IAgentInitializationService
{
    private readonly AgentRegistry _registry;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AgentStartupService> _logger;

    public AgentStartupService(
        AgentRegistry registry,
        IServiceScopeFactory scopeFactory,
        ILogger<AgentStartupService> logger)
    {
        _registry = registry;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InitializeAllAgentsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var repository = serviceProvider.GetRequiredService<IAgentRepository>();
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        _logger.LogInformation("Loading existing agents from database...");
        var agents = await repository.GetAllAsync(cancellationToken);

        foreach (var agent in agents)
        {
            var runtimeAgent = new AgentBase(agent, eventBus, mediator, _logger);
            _registry.Register(runtimeAgent);
            _logger.LogInformation("Agent {Name} loaded and registered", agent.Name.Value);
        }

        _logger.LogInformation("AI Office OS operational with {Count} loaded agents", agents.Count);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting agent workforce...");
        await InitializeAllAgentsAsync(stoppingToken);
        _logger.LogInformation("Agent workforce ready with {Count} agents", _registry.Count);
    }
}
