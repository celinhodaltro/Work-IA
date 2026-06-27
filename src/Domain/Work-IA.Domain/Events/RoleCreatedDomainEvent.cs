using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Roles;

namespace Work_IA.Domain.Events;

public sealed record RoleCreatedDomainEvent : DomainEvent
{
    public RoleId RoleId { get; }
    public string Name { get; }

    public RoleCreatedDomainEvent(RoleId roleId, string name)
    {
        RoleId = roleId;
        Name = name;
    }
}
