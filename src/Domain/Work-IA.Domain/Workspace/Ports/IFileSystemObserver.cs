namespace Work_IA.Domain.Workspace.Ports;

public interface IFileSystemObserver : IDisposable
{
    event EventHandler<FileChangeInfo>? FileChanged;
    event EventHandler<Exception>? OnError;
    Task StartAsync(WorkspacePath path, CancellationToken cancellationToken = default);
    Task StopAsync();
    bool IsObserving { get; }
}
