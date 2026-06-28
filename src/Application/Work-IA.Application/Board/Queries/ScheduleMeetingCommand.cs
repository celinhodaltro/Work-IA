using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Board.Queries;

public sealed record ScheduleMeetingCommand(string Topic, List<Guid> ParticipantIds) : IRequest<Guid>;
