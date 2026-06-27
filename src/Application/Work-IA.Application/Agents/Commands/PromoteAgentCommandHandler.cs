using MediatR;
using Work_IA.Application.Common.Exceptions;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class PromoteAgentCommandHandler : IRequestHandler<PromoteAgentCommand>
{
    private readonly IAgentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public PromoteAgentCommandHandler(
        IAgentRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PromoteAgentCommand request, CancellationToken cancellationToken)
    {
        var agent = await _repository.GetByIdAsync(request.AgentId, cancellationToken);
        if (agent is null)
            throw new NotFoundException(nameof(Agent), request.AgentId.Value);

        var xpRequired = GetXpRequirement(request.NewLevel);

        if (!agent.CanPromoteTo(request.NewLevel, xpRequired))
            throw new InvalidOperationException($"Agent {agent.Name.Value} cannot promote to {request.NewLevel}. XP: {agent.ExperiencePoints}, Required: {xpRequired}");

        agent.Promote(new AgentTitle(request.NewTitle), request.NewLevel);
        await _repository.UpdateAsync(agent, cancellationToken);
    }

    private static int GetXpRequirement(AgentCareerLevel level) => level switch
    {
        AgentCareerLevel.Intern => 0,
        AgentCareerLevel.Junior => 100,
        AgentCareerLevel.Pleno => 500,
        AgentCareerLevel.Senior => 1000,
        AgentCareerLevel.TechLead => 3000,
        AgentCareerLevel.Architect => 6000,
        AgentCareerLevel.Head => 10000,
        _ => 0
    };
}
