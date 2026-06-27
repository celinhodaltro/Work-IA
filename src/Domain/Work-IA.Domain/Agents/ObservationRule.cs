using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class ObservationRule : ValueObject
{
    public string EventType { get; }
    public ObservationPriority Priority { get; }
    public Func<object, bool>? Condition { get; }

    public ObservationRule(string eventType, ObservationPriority priority, Func<object, bool>? condition = null)
    {
        EventType = eventType;
        Priority = priority;
        Condition = condition;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EventType;
        yield return Priority;
    }
}
