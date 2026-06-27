using Work_IA.Application.Common.Interfaces;

namespace Work_IA.Infrastructure.Workspace;

public sealed class FileSystemService : IFileSystemService
{
    private readonly string _workspacePath;

    public FileSystemService(string workspacePath)
    {
        _workspacePath = workspacePath;
    }

    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_workspacePath, path);
        return await File.ReadAllTextAsync(fullPath, cancellationToken);
    }

    public async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_workspacePath, path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(fullPath, content, cancellationToken);
    }

    public Task<bool> FileExistsAsync(string path)
    {
        var fullPath = Path.Combine(_workspacePath, path);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<string[]> GetFilesAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var files = Directory.GetFiles(_workspacePath, pattern, SearchOption.AllDirectories);
        return Task.FromResult(files);
    }

    public string GetRelativePath(string fullPath)
    {
        if (fullPath.StartsWith(_workspacePath, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath[_workspacePath.Length..].TrimStart(Path.DirectorySeparatorChar);
        }

        return fullPath;
    }
}
