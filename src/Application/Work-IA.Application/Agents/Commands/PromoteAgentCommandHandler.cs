using MediatR;
using Work_IA.Application.Common.Exceptions;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Application.Configuration;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class PromoteAgentCommandHandler : IRequestHandler<PromoteAgentCommand>
{
    private readonly IAgentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RoleDefinitionProvider _roleProvider;

    public PromoteAgentCommandHandler(
        IAgentRepository repository,
        IUnitOfWork unitOfWork,
        RoleDefinitionProvider roleProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _roleProvider = roleProvider;
    }

    public async Task Handle(PromoteAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _repository.GetByIdAsync(request.AgentId, cancellationToken);
        if (agent is null)
            throw new NotFoundException(nameof(Agent), request.AgentId.Value);

        var roleDef = _roleProvider.GetByLevel(request.NewLevel);

        if (!agent.CanPromoteTo(request.NewLevel, roleDef?.XpRequired ?? 0))
            throw new InvalidOperationException($"Agent {agent.Name.Value} cannot promote to {request.NewLevel}. XP: {agent.ExperiencePoints}, Required: {roleDef?.XpRequired}");

        agent.Promote(new AgentTitle(request.NewTitle), request.NewLevel);
        await _repository.UpdateAsync(agent, cancellationToken);
    }
}
