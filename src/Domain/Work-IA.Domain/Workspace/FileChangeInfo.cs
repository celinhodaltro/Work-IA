using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Workspace;

public sealed class FileChangeInfo : ValueObject
{
    public WorkspacePath Path { get; }
    public FileChangeType Type { get; }
    public WorkspacePath? OldPath { get; }
    public string? OldContentHash { get; }
    public string? NewContentHash { get; }
    public long? OldSizeBytes { get; }
    public long? NewSizeBytes { get; }
    public DateTime? OldLastModified { get; }
    public DateTime NewLastModified { get; }

    public FileChangeInfo(
        WorkspacePath path,
        FileChangeType type,
        WorkspacePath? oldPath = null,
        string? oldContentHash = null,
        string? newContentHash = null,
        long? oldSizeBytes = null,
        long? newSizeBytes = null,
        DateTime? oldLastModified = null,
        DateTime? newLastModified = null)
    {
        Path = path;
        Type = type;
        OldPath = oldPath;
        OldContentHash = oldContentHash;
        NewContentHash = newContentHash;
        OldSizeBytes = oldSizeBytes;
        NewSizeBytes = newSizeBytes;
        OldLastModified = oldLastModified;
        NewLastModified = newLastModified ?? DateTime.UtcNow;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Path;
        yield return Type;
        yield return OldPath ?? new WorkspacePath("");
        yield return OldContentHash ?? string.Empty;
        yield return NewContentHash ?? string.Empty;
        yield return OldSizeBytes ?? 0;
        yield return NewSizeBytes ?? 0;
    }
}
