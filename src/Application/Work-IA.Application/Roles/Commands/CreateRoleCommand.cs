using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Roles.Commands;

public sealed record CreateRoleCommand(
    string Name,
    List<string>? Technologies,
    List<string>? Methodologies,
    List<string>? Tools) : IRequest<RoleId>, IRequiresUnitOfWork;
