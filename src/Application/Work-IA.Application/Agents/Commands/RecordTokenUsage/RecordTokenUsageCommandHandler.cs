using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class RecordTokenUsageCommandHandler : IRequestHandler<RecordTokenUsageCommand>
{
    private readonly ITokenUsageRepository _repository;

    public RecordTokenUsageCommandHandler(ITokenUsageRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(RecordTokenUsageCommand request, CancellationToken cancellationToken)
    {
        var usage = TokenUsage.Record(request.AgentId, request.Model, request.TokensIn, request.TokensOut, request.Task);
        await _repository.AddAsync(usage, cancellationToken);
    }
}
