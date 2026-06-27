using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class AgentSkill : ValueObject
{
    public string Name { get; }
    public int Proficiency { get; }

    public AgentSkill(string name, int proficiency = 10)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Skill name cannot be empty", nameof(name));
        Name = name;
        Proficiency = Math.Clamp(proficiency, 1, 100);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name.ToLowerInvariant();
    }
}
