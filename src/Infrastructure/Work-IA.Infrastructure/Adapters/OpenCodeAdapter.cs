using System.Reactive.Subjects;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Events;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workspace;
using Work_IA.Domain.Workspace.Ports;

namespace Work_IA.Infrastructure.Adapters;

public sealed class OpenCodeAdapter : IWorkspaceAdapter, IOpenCodeOperations, IFileChangeObservable
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<OpenCodeAdapter> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _connected;
    private readonly Subject<WorkspaceFileChange> _fileChanges = new();

    public string PlatformName => "OpenCode";

    public IObservable<WorkspaceFileChange> FileChanges => _fileChanges;

    public OpenCodeAdapter(IEventBus eventBus, ILogger<OpenCodeAdapter> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public Task<IReadOnlyList<WorkspaceFile>> GetFilesAsync(string pattern = "*", CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("OpenCodeAdapter: GetFilesAsync called with pattern {Pattern}", pattern);
        return Task.FromResult<IReadOnlyList<WorkspaceFile>>([]);
    }

    public Task<WorkspaceFile?> ReadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("OpenCodeAdapter: ReadFileAsync called for {Path}", path);
        return Task.FromResult<WorkspaceFile?>(null);
    }

    public Task WriteFileAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("OpenCodeAdapter: WriteFileAsync called for {Path}", path);
        return Task.CompletedTask;
    }

    public Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("OpenCodeAdapter: DeleteFileAsync called for {Path}", path);
        return Task.CompletedTask;
    }

    public Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_connected);
    }

    public async Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("OpenCodeAdapter: Connecting to {Endpoint}", endpoint);

        try
        {
            await Task.Delay(100, cancellationToken);
            _connected = true;
            _logger.LogInformation("OpenCodeAdapter: Connected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenCodeAdapter: Connection failed");
            _connected = false;
            throw;
        }
    }

    public Task DisconnectAsync()
    {
        _connected = false;
        _logger.LogInformation("OpenCodeAdapter: Disconnected");
        return Task.CompletedTask;
    }

    public async Task ProcessEventAsync(string eventType, string eventData, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<OpenCodeEventType>(eventType, true, out var parsedType))
        {
            _logger.LogWarning("OpenCodeAdapter: Unknown event type {EventType}", eventType);
            return;
        }

        IIntegrationEvent? integrationEvent = parsedType switch
        {
            OpenCodeEventType.FileCreated or OpenCodeEventType.FileChanged => HandleFileEvent(parsedType, eventData),
            OpenCodeEventType.ToolStarted or OpenCodeEventType.ToolCompleted => HandleToolEvent(parsedType, eventData),
            OpenCodeEventType.GitCommit => HandleGitEvent(eventData),
            _ => null
        };

        if (integrationEvent is not null)
        {
            await _eventBus.PublishAsync(integrationEvent, cancellationToken);
        }
    }

    private WorkspaceFileChange? HandleFileEvent(OpenCodeEventType type, string data)
    {
        try
        {
            var fileData = JsonSerializer.Deserialize<OpenCodeFileData>(data, _jsonOptions);
            if (fileData is null) return null;

            var changeType = type == OpenCodeEventType.FileCreated ? FileChangeType.Created : FileChangeType.Modified;

            var change = new WorkspaceFileChange
            {
                Path = fileData.Path,
                ChangeType = changeType,
                OldContent = fileData.OldContent,
                NewContent = fileData.NewContent
            };

            _fileChanges.OnNext(change);

            return change;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenCodeAdapter: Failed to handle file event");
            return null;
        }
    }

    private IIntegrationEvent? HandleToolEvent(OpenCodeEventType type, string data)
    {
        return null;
    }

    private IIntegrationEvent? HandleGitEvent(string data)
    {
        return null;
    }

    public void Dispose()
    {
        _fileChanges.Dispose();
    }
}

internal sealed class OpenCodeFileData
{
    public string Path { get; set; } = string.Empty;
    public string? OldContent { get; set; }
    public string? NewContent { get; set; }
}
