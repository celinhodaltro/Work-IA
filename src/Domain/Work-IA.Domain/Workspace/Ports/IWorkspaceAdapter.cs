using Work_IA.Domain.Workspace;

namespace Work_IA.Domain.Workspace.Ports;

public interface IWorkspaceAdapter
{
    string PlatformName { get; }
    Task<IReadOnlyList<WorkspaceFile>> GetFilesAsync(string pattern = "*", CancellationToken cancellationToken = default);
    Task<WorkspaceFile?> ReadFileAsync(string path, CancellationToken cancellationToken = default);
    Task WriteFileAsync(string path, string content, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
}
