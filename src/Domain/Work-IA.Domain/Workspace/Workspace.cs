using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Workspace;

public sealed class Workspace : AggregateRoot<WorkspaceId>
{
    public WorkspacePath Path { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastAccessedAt { get; private set; }

    private Workspace(WorkspaceId id, WorkspacePath path, string? description) : base(id)
    {
        Path = path;
        Description = description;
        IsActive = true;
        LastAccessedAt = DateTime.UtcNow;
    }

    public static Workspace Create(WorkspacePath path, string? description = null)
    {
        return new Workspace(WorkspaceId.New(), path, description);
    }

    public void UpdateLastAccessed()
    {
        LastAccessedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
