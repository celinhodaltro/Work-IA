using MediatR;
using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Roles.Commands;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleId>
{
    private readonly IRoleRepository _repository;

    public CreateRoleCommandHandler(IRoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<RoleId> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = RoleDefinition.Create(request.Name, request.Technologies, request.Methodologies, request.Tools);
        await _repository.AddAsync(role, cancellationToken);
        return role.RoleId;
    }
}
