namespace Work_IA.Application.Common.Interfaces;

public interface IIntegrationEvent
{
    string EventId { get; }
    string EventType { get; }
    DateTime OccurredOn { get; }
}
