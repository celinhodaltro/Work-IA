namespace Work_IA.Infrastructure.Adapters;

public interface IOpenCodeOperations
{
    Task ConnectAsync(string endpoint, CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);
    Task ProcessEventAsync(string eventType, string eventData, CancellationToken cancellationToken = default);
}
