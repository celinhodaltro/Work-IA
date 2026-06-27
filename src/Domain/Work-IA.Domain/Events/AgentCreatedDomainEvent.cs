using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record AgentCreatedDomainEvent : DomainEvent
{
    public AgentId AgentId { get; }
    public string AgentName { get; }
    public AgentRole AgentRole { get; }

    public AgentCreatedDomainEvent(AgentId agentId, string agentName, AgentRole agentRole)
    {
        AgentId = agentId;
        AgentName = agentName;
        AgentRole = agentRole;
    }
}
