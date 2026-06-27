using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workspace;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Infrastructure.Adapters;

public sealed class FileSystemWorkspaceAdapter : IWorkspaceAdapter
{
    private readonly IFileSystemService _fileSystem;
    private readonly ILogger<FileSystemWorkspaceAdapter> _logger;

    public string PlatformName => "FileSystem";

    public FileSystemWorkspaceAdapter(IFileSystemService fileSystem, ILogger<FileSystemWorkspaceAdapter> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WorkspaceFile>> GetFilesAsync(string pattern = "*", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FileSystemAdapter: GetFilesAsync with pattern {Pattern}", pattern);
        var paths = await _fileSystem.GetFilesAsync(pattern, cancellationToken);
        return paths.Select(p => new WorkspaceFile(
            new WorkspacePath(_fileSystem.GetRelativePath(p)),
            null,
            DateTime.MinValue,
            0)).ToList();
    }

    public async Task<WorkspaceFile?> ReadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FileSystemAdapter: ReadFileAsync for {Path}", path);
        if (!await _fileSystem.FileExistsAsync(path))
            return null;

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
        var fileInfo = new FileInfo(fullPath);
        return new WorkspaceFile(
            new WorkspacePath(path),
            null,
            fileInfo.Exists ? fileInfo.LastWriteTimeUtc : DateTime.MinValue,
            fileInfo.Exists ? fileInfo.Length : 0);
    }

    public async Task WriteFileAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FileSystemAdapter: WriteFileAsync for {Path}", path);
        await _fileSystem.WriteAllTextAsync(path, content, cancellationToken);
    }

    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FileSystemAdapter: DeleteFileAsync for {Path}", path);
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
