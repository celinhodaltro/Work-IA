using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record ObservationRuleAddedDomainEvent : DomainEvent
{
    public AgentId AgentId { get; }
    public new string EventType { get; }

    public ObservationRuleAddedDomainEvent(AgentId agentId, string eventType)
    {
        AgentId = agentId;
        EventType = eventType;
    }
}
