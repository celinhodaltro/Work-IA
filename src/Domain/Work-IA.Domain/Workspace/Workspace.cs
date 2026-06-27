using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Events;

namespace Work_IA.Domain.Workspace;

public sealed class Workspace : AggregateRoot<WorkspaceId>
{
    private readonly List<WorkspaceFile> _files = [];

    public IReadOnlyList<WorkspaceFile> Files => _files.AsReadOnly();
    public WorkspacePath Path { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastAccessedAt { get; private set; }

    private Workspace() { }

    public Workspace(WorkspaceId id, WorkspacePath path, string? description = null)
    {
        Id = id;
        Path = path;
        Description = description;
        IsActive = true;
    }

    public void RecordFileChange(WorkspacePath path, FileChangeType type, FileChangeInfo? changeInfo = null)
    {
        if (!IsActive)
            throw new InvalidOperationException("Cannot modify inactive workspace");

        switch (type)
        {
            case FileChangeType.Created:
                if (_files.Any(f => f.Path.Equals(path)))
                    return;
                if (changeInfo is not null)
                {
                    _files.Add(new WorkspaceFile(
                        path,
                        changeInfo.NewContentHash,
                        changeInfo.NewLastModified,
                        changeInfo.NewSizeBytes ?? 0));
                }
                break;

            case FileChangeType.Modified:
                _files.RemoveAll(f => f.Path.Equals(path));
                if (changeInfo is not null)
                {
                    _files.Add(new WorkspaceFile(
                        path,
                        changeInfo.NewContentHash,
                        changeInfo.NewLastModified,
                        changeInfo.NewSizeBytes ?? 0));
                }
                break;

            case FileChangeType.Deleted:
                _files.RemoveAll(f => f.Path.Equals(path));
                break;

            case FileChangeType.Renamed:
                if (changeInfo?.OldPath is not null)
                {
                    _files.RemoveAll(f => f.Path.Equals(changeInfo.OldPath));
                }
                _files.RemoveAll(f => f.Path.Equals(path));
                if (changeInfo is not null)
                {
                    _files.Add(new WorkspaceFile(
                        path,
                        changeInfo.NewContentHash,
                        changeInfo.NewLastModified,
                        changeInfo.NewSizeBytes ?? 0));
                }
                break;
        }

        LastAccessedAt = DateTime.UtcNow;
        IncrementVersion();
        RaiseDomainEvent(new WorkspaceFileChangedDomainEvent(Id, path, type, changeInfo));
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void RecordAccess()
    {
        LastAccessedAt = DateTime.UtcNow;
    }
}
