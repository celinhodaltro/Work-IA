using System.Reactive.Subjects;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Events;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workspace;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Infrastructure.Adapters;

public sealed class ClaudeCodeAdapter : IWorkspaceAdapter, IFileChangeObservable
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<ClaudeCodeAdapter> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _connected;
    private readonly Subject<WorkspaceFileChange> _fileChanges = new();

    public string PlatformName => "Claude Code";

    public IObservable<WorkspaceFileChange> FileChanges => _fileChanges;

    public ClaudeCodeAdapter(IEventBus eventBus, ILogger<ClaudeCodeAdapter> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public Task<IReadOnlyList<WorkspaceFile>> GetFilesAsync(string pattern = "*", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ClaudeCodeAdapter: GetFilesAsync({Pattern})", pattern);
        return Task.FromResult<IReadOnlyList<WorkspaceFile>>([]);
    }

    public Task<WorkspaceFile?> ReadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ClaudeCodeAdapter: ReadFileAsync({Path})", path);
        return Task.FromResult<WorkspaceFile?>(null);
    }

    public Task WriteFileAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ClaudeCodeAdapter: WriteFileAsync({Path})", path);
        return Task.CompletedTask;
    }

    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ClaudeCodeAdapter: DeleteFileAsync({Path})", path);
        return Task.CompletedTask;
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_connected);
    }

    public async Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ClaudeCodeAdapter: Connecting to {Endpoint}", endpoint);
        await Task.Delay(100, cancellationToken);
        _connected = true;
        _logger.LogInformation("ClaudeCodeAdapter: Connected");
    }

    public Task DisconnectAsync()
    {
        _connected = false;
        return Task.CompletedTask;
    }
}
