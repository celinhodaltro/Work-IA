using MediatR;
using Work_IA.Application.Agents;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, AgentId>
{
    private readonly IAgentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AgentRegistry _agentRegistry;
    private readonly AgentRoleFactory _agentRoleFactory;

    public CreateAgentCommandHandler(
        IAgentRepository repository,
        IUnitOfWork unitOfWork,
        AgentRegistry agentRegistry,
        AgentRoleFactory agentRoleFactory)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _agentRegistry = agentRegistry;
        _agentRoleFactory = agentRoleFactory;
    }

    public async Task<AgentId> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = Agent.Create(new AgentName(request.Name), request.Role);

        await _repository.AddAsync(agent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var agentInterface = _agentRoleFactory.Create(request.Role);
        _agentRegistry.Register(agentInterface);

        return agent.Id;
    }
}
