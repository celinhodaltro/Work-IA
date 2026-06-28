using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Agents;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, AgentId>
{
    private readonly IAgentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AgentRegistry _agentRegistry;
    private readonly IEventBus _eventBus;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateAgentCommandHandler> _logger;

    public CreateAgentCommandHandler(
        IAgentRepository repository,
        IUnitOfWork unitOfWork,
        AgentRegistry agentRegistry,
        IEventBus eventBus,
        IMediator mediator,
        ILogger<CreateAgentCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _agentRegistry = agentRegistry;
        _eventBus = eventBus;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<AgentId> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        RoleId? roleId = request.RoleId.HasValue ? RoleId.From(request.RoleId.Value) : null;
        var agent = Agent.Create(new AgentName(request.Name), new AgentTitle(request.Title), roleId);

        await _repository.AddAsync(agent, cancellationToken);

        var runtimeAgent = new AgentBase(agent, _eventBus, _mediator, _logger);
        _agentRegistry.Register(runtimeAgent);

        return agent.AgentId;
    }
}
