using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Queries;

public sealed class GetAgentCostQueryHandler : IRequestHandler<GetAgentCostQuery, AgentCostSummary?>
{
    private readonly ITokenUsageRepository _repository;

    public GetAgentCostQueryHandler(ITokenUsageRepository repository)
    {
        _repository = repository;
    }

    public async Task<AgentCostSummary?> Handle(GetAgentCostQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAgentSummaryAsync(request.AgentId, request.Days, cancellationToken);
    }
}
