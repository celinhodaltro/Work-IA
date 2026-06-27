using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record AgentStartedDomainEvent : DomainEvent
{
    public AgentId AgentId { get; }

    public AgentStartedDomainEvent(AgentId agentId)
    {
        AgentId = agentId;
    }
}
