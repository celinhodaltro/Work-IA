using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Queries;

public sealed class GetAllAgentsCostQueryHandler : IRequestHandler<GetAllAgentsCostQuery, List<AgentCostSummary>>
{
    private readonly ITokenUsageRepository _repository;

    public GetAllAgentsCostQueryHandler(ITokenUsageRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AgentCostSummary>> Handle(GetAllAgentsCostQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAgentsSummaryAsync(request.Days, cancellationToken);
    }
}
