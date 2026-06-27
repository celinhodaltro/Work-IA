using MediatR;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Queries;

public sealed class GetAgentQueryHandler : IRequestHandler<GetAgentQuery, Agent?>
{
    private readonly IAgentRepository _repository;

    public GetAgentQueryHandler(IAgentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Agent?> Handle(GetAgentQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id, cancellationToken);
    }
}
