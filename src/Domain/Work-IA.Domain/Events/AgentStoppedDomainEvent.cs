using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record AgentStoppedDomainEvent : DomainEvent
{
    public AgentId AgentId { get; }

    public AgentStoppedDomainEvent(AgentId agentId)
    {
        AgentId = agentId;
    }
}
