using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Workspace;

namespace Work_IA.Domain.Events;

public sealed record WorkspaceFileChangedDomainEvent : DomainEvent
{
    public WorkspaceId WorkspaceId { get; }
    public WorkspacePath Path { get; }
    public FileChangeType ChangeType { get; }
    public FileChangeInfo? ChangeInfo { get; }

    public WorkspaceFileChangedDomainEvent(
        WorkspaceId workspaceId,
        WorkspacePath path,
        FileChangeType changeType,
        FileChangeInfo? changeInfo = null)
    {
        WorkspaceId = workspaceId;
        Path = path;
        ChangeType = changeType;
        ChangeInfo = changeInfo;
    }
}
