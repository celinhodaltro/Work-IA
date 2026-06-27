using MediatR;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Queries;

public sealed class ListAgentsQueryHandler : IRequestHandler<ListAgentsQuery, IReadOnlyList<Agent>>
{
    private readonly IAgentRepository _repository;

    public ListAgentsQueryHandler(IAgentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Agent>> Handle(ListAgentsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}
