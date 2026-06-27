using MediatR;
using Work_IA.Domain.Roles;

namespace Work_IA.Application.Roles.Queries;

public sealed record ListRolesQuery : IRequest<List<RoleDefinition>>;
