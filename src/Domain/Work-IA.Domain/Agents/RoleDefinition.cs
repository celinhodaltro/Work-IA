using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class RoleDefinition : ValueObject
{
    public string Title { get; }
    public AgentCareerLevel Level { get; }
    public IReadOnlyList<string> Permissions { get; }
    public int XpRequired { get; }

    public RoleDefinition(string title, AgentCareerLevel level, List<string> permissions, int xpRequired = 0)
    {
        Title = title;
        Level = level;
        Permissions = permissions ?? [];
        XpRequired = xpRequired;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Title.ToLowerInvariant();
    }
}
