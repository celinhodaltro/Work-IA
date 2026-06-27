using MediatR;
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
    private readonly IAgentRepository _repository;
    private readonly IEventBus _eventBus;
    private readonly IMediator _mediator;
    private readonly ILogger<AgentStartupService> _logger;

    public AgentStartupService(
        AgentRegistry registry,
        IAgentRepository repository,
        IEventBus eventBus,
        IMediator mediator,
        ILogger<AgentStartupService> logger)
    {
        _registry = registry;
        _repository = repository;
        _eventBus = eventBus;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task InitializeAllAgentsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Loading existing agents from database...");

        var agents = await _repository.GetAllAsync(cancellationToken);

        foreach (var agent in agents)
        {
            var runtimeAgent = new AgentBase(agent, _eventBus, _mediator, _logger);
            _registry.Register(runtimeAgent);
            _logger.LogInformation("Agent {Name} loaded and registered", agent.Name.Value);
        }

        _logger.LogInformation("AI Office OS operational with {Count} loaded agents", agents.Count);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeAllAgentsAsync(stoppingToken);
    }
}
