namespace Work_IA.Domain.Agents;

public readonly record struct AgentId(Guid Value)
{
    public static AgentId New() => new(Guid.NewGuid());
    public static AgentId From(Guid value) => new(value);
}
