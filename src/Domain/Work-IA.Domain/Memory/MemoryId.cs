namespace Work_IA.Domain.Memory;

public readonly record struct MemoryId(Guid Value)
{
    public static MemoryId New() => new(Guid.NewGuid());
    public static MemoryId From(Guid value) => new(value);
}
