namespace Work_IA.Infrastructure.Persistence;

public sealed class StoredEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string AggregateType { get; set; } = string.Empty;
    public Guid? AggregateId { get; set; }
    public string Data { get; set; } = string.Empty;
    public DateTime OccurredOn { get; set; }
}
