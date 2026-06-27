namespace Work_IA.Application.Common.Interfaces;

public sealed class GitCommitInfo
{
    public string Hash { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public interface IGitIntegrationService
{
    Task<GitCommitInfo?> GetLastCommitAsync(CancellationToken cancellationToken = default);
    Task<string> GetCurrentBranchAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetChangedFilesAsync(CancellationToken cancellationToken = default);
}
