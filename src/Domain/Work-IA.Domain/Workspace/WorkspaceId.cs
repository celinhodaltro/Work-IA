namespace Work_IA.Domain.Workspace;

public readonly record struct WorkspaceId(Guid Value)
{
    public static WorkspaceId New() => new(Guid.NewGuid());
    public static WorkspaceId From(Guid value) => new(value);
}
