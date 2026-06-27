using System.Collections.Concurrent;
using Work_IA.Domain.Workspace;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Infrastructure.Workspace;

public sealed class FileSystemWatcherObserver : IFileSystemObserver
{
    private FileSystemWatcher? _watcher;
    private readonly List<string> _ignorePatterns;
    private readonly ConcurrentDictionary<string, DateTime> _recentChanges = new();
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500);
    private bool _disposed;

    public event EventHandler<FileChangeInfo>? FileChanged;
    public event EventHandler<Exception>? OnError;
    public bool IsObserving => _watcher?.EnableRaisingEvents ?? false;

    public FileSystemWatcherObserver(List<string>? ignorePatterns = null)
    {
        _ignorePatterns = ignorePatterns ??
        [
            "node_modules", "bin", "obj", ".git", ".agent-memory",
            ".vs", ".vscode", "*.user", "*.suo"
        ];
    }

    public Task StartAsync(WorkspacePath path, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(path.Value))
            throw new DirectoryNotFoundException($"Workspace path not found: {path.Value}");

        _watcher = new FileSystemWatcher(path.Value)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        _watcher.Created += (s, e) => HandleChange(e.FullPath, FileChangeType.Created);
        _watcher.Changed += (s, e) => HandleChange(e.FullPath, FileChangeType.Modified);
        _watcher.Deleted += (s, e) => HandleChange(e.FullPath, FileChangeType.Deleted);
        _watcher.Renamed += (s, e) => HandleRename(e.FullPath, e.OldFullPath);
        _watcher.Error += (s, e) => OnError?.Invoke(this, e.GetException());

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }
        return Task.CompletedTask;
    }

    private void HandleChange(string fullPath, FileChangeType type)
    {
        if (ShouldIgnore(fullPath))
            return;

        var now = DateTime.UtcNow;
        if (_recentChanges.TryGetValue(fullPath, out var lastChange) && (now - lastChange) < _debounceInterval)
            return;

        _recentChanges[fullPath] = now;

        var oldHash = default(string?);
        var oldSize = default(long?);
        var oldModified = default(DateTime?);

        if (type == FileChangeType.Modified && File.Exists(fullPath))
        {
            try
            {
                var oldInfo = new FileInfo(fullPath);
                oldSize = oldInfo.Exists ? oldInfo.Length : (long?)null;
                oldModified = oldInfo.Exists ? oldInfo.LastWriteTimeUtc : null;
            }
            catch { }
        }

        var fileInfo = new FileInfo(fullPath);
        var newHash = fileInfo.Exists ? ComputeHash(fullPath) : null;
        var newSize = fileInfo.Exists ? fileInfo.Length : (long?)null;
        var newModified = fileInfo.Exists ? fileInfo.LastWriteTimeUtc : DateTime.UtcNow;

        var changeInfo = new FileChangeInfo(
            path: new WorkspacePath(fullPath),
            type: type,
            oldContentHash: oldHash,
            newContentHash: newHash,
            oldSizeBytes: oldSize,
            newSizeBytes: newSize,
            oldLastModified: oldModified,
            newLastModified: newModified);

        FileChanged?.Invoke(this, changeInfo);
    }

    private void HandleRename(string fullPath, string oldFullPath)
    {
        if (ShouldIgnore(fullPath) && ShouldIgnore(oldFullPath))
            return;

        var now = DateTime.UtcNow;
        if (_recentChanges.TryGetValue(fullPath, out var lastChange) && (now - lastChange) < _debounceInterval)
            return;

        _recentChanges[fullPath] = now;

        var oldInfo = new FileInfo(oldFullPath);
        var newInfo = new FileInfo(fullPath);

        var changeInfo = new FileChangeInfo(
            path: new WorkspacePath(fullPath),
            type: FileChangeType.Renamed,
            oldPath: new WorkspacePath(oldFullPath),
            oldContentHash: oldInfo.Exists ? ComputeHash(oldFullPath) : null,
            newContentHash: newInfo.Exists ? ComputeHash(fullPath) : null,
            oldSizeBytes: oldInfo.Exists ? oldInfo.Length : null,
            newSizeBytes: newInfo.Exists ? newInfo.Length : null,
            oldLastModified: oldInfo.Exists ? oldInfo.LastWriteTimeUtc : null,
            newLastModified: newInfo.Exists ? newInfo.LastWriteTimeUtc : DateTime.UtcNow);

        FileChanged?.Invoke(this, changeInfo);
    }

    private bool ShouldIgnore(string path)
    {
        return _ignorePatterns.Any(pattern =>
            path.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ComputeHash(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            var hash = System.Security.Cryptography.SHA256.HashData(stream);
            return Convert.ToHexString(hash);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopAsync().GetAwaiter().GetResult();
        _disposed = true;
    }
}
