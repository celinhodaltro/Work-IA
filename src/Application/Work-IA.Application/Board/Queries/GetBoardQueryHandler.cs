using MediatR;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Ports;

namespace Work_IA.Application.Board.Queries;

public sealed class GetBoardQueryHandler : IRequestHandler<GetBoardQuery, List<BoardTask>>
{
    private readonly IBoardRepository _repository;
    public GetBoardQueryHandler(IBoardRepository repository) => _repository = repository;
    public async Task<List<BoardTask>> Handle(GetBoardQuery request, CancellationToken ct)
        => await _repository.GetAllAsync(ct);
}
