namespace Work_IA.Application.Common.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IIntegrationEvent;
    Task SubscribeAsync<T>(string subscriberId, Func<T, CancellationToken, Task> handler) where T : IIntegrationEvent;
    Task UnsubscribeAsync<T>(string subscriberId) where T : IIntegrationEvent;
}
