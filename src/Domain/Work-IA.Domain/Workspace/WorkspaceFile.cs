using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Workspace;

public sealed class WorkspaceFile : ValueObject
{
    public WorkspacePath Path { get; }
    public string? ContentHash { get; }
    public DateTime LastModified { get; }
    public long SizeBytes { get; }

    public WorkspaceFile(WorkspacePath path, string? contentHash, DateTime lastModified, long sizeBytes)
    {
        Path = path;
        ContentHash = contentHash;
        LastModified = lastModified;
        SizeBytes = sizeBytes;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Path;
        yield return ContentHash ?? string.Empty;
    }
}
