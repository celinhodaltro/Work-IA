using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Board.Queries;

public sealed record GetBoardQuery : IRequest<List<BoardTask>>;
