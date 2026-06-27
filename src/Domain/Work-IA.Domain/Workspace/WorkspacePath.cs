using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Workspace;

public sealed class WorkspacePath : ValueObject
{
    public string Value { get; }

    public WorkspacePath(string value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }

    public override string ToString() => Value;

    public bool IsEmpty => string.IsNullOrWhiteSpace(Value);
}
