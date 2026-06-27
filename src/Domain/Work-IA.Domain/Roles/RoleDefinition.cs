using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Events;

namespace Work_IA.Domain.Roles;

public sealed class RoleDefinition : AggregateRoot<RoleId>
{
    public RoleId RoleId { get; private set; }
    public string Name { get; private set; }
    public List<string> Technologies { get; private set; }
    public List<string> Methodologies { get; private set; }
    public List<string> Tools { get; private set; }

    private RoleDefinition() : base(RoleId.New()) { }

    public static RoleDefinition Create(string name, List<string>? technologies = null, List<string>? methodologies = null, List<string>? tools = null)
    {
        var role = new RoleDefinition
        {
            RoleId = RoleId.New(),
            Name = name,
            Technologies = technologies ?? [],
            Methodologies = methodologies ?? [],
            Tools = tools ?? []
        };
        role.RaiseDomainEvent(new RoleCreatedDomainEvent(role.RoleId, name));
        return role;
    }

    public static RoleDefinition Hydrate(string name, List<string>? technologies = null, List<string>? methodologies = null, List<string>? tools = null)
    {
        var role = new RoleDefinition
        {
            RoleId = RoleId.New(),
            Name = name,
            Technologies = technologies ?? [],
            Methodologies = methodologies ?? [],
            Tools = tools ?? []
        };
        return role;
    }

    public void AddTechnology(string tech)
    {
        if (!Technologies.Contains(tech))
            Technologies.Add(tech);
    }

    public void AddMethodology(string method)
    {
        if (!Methodologies.Contains(method))
            Methodologies.Add(method);
    }

    public void AddTool(string tool)
    {
        if (!Tools.Contains(tool))
            Tools.Add(tool);
    }
}
