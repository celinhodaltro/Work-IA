namespace Work_IA.Domain.Workflows;

public readonly record struct WorkflowId(Guid Value)
{
    public static WorkflowId New() => new(Guid.NewGuid());
    public static WorkflowId From(Guid value) => new(value);
}
