using MediatR;
using Work_IA.Application.Common.Exceptions;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class AddExperienceCommandHandler : IRequestHandler<AddExperienceCommand>
{
    private readonly IAgentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddExperienceCommandHandler(IAgentRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddExperienceCommand request, CancellationToken cancellationToken)
    {
        var agent = await _repository.GetByIdAsync(request.AgentId, cancellationToken);
        if (agent is null)
            throw new NotFoundException(nameof(Agent), request.AgentId.Value);

        agent.AddExperience(request.Points);
        await _repository.UpdateAsync(agent, cancellationToken);
    }
}
