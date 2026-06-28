using MediatR;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Board.Commands;

public sealed record CreateTaskCommand(string Title, string Description, TaskPriority Priority = TaskPriority.Normal) : IRequest<Guid>;
