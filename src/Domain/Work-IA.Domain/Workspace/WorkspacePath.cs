using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Workspace;

public sealed class WorkspacePath : ValueObject
{
    public string Value { get; }

    public WorkspacePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Path cannot be empty", nameof(value));

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
