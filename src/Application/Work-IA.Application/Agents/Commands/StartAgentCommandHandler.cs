using MediatR;
using Work_IA.Application.Agents;
using Work_IA.Application.Common.Exceptions;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class StartAgentCommandHandler : IRequestHandler<StartAgentCommand>
{
    private readonly IAgentRepository _repository;
    private readonly AgentRegistry _agentRegistry;

    public StartAgentCommandHandler(IAgentRepository repository, AgentRegistry agentRegistry)
    {
        _repository = repository;
        _agentRegistry = agentRegistry;
    }

    public async Task Handle(StartAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _repository.GetByIdAsync(request.AgentId, cancellationToken);

        if (agent is null)
            throw new NotFoundException(nameof(Agent), request.AgentId.Value);

        agent.Start();

        await _repository.UpdateAsync(agent, cancellationToken);

        var runtimeAgent = _agentRegistry.Get(request.AgentId);
        if (runtimeAgent is not null)
            await runtimeAgent.StartAsync(cancellationToken);
    }
}
