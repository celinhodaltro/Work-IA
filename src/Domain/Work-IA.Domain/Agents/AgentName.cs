using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class AgentName : ValueObject
{
    public string Value { get; }

    public AgentName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Agent name cannot be empty", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Agent name cannot exceed 100 characters", nameof(value));

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
