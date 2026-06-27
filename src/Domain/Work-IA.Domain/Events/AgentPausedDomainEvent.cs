using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record AgentPausedDomainEvent : DomainEvent
{
    public AgentId AgentId { get; }

    public AgentPausedDomainEvent(AgentId agentId)
    {
        AgentId = agentId;
    }
}
