using MediatR;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Roles.Queries;

public sealed class ListRolesQueryHandler : IRequestHandler<ListRolesQuery, List<RoleDefinition>>
{
    private readonly IRoleRepository _repository;

    public ListRolesQueryHandler(IRoleRepository repository) => _repository = repository;

    public async Task<List<RoleDefinition>> Handle(ListRolesQuery request, CancellationToken ct)
        => await _repository.GetAllAsync(ct);
}
