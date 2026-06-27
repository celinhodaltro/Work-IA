using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record AgentCreatedDomainEvent : DomainEvent
{
    public AgentId AgentId { get; }
    public string AgentName { get; }
    public string AgentTitle { get; }

    public AgentCreatedDomainEvent(AgentId agentId, string agentName, string agentTitle)
    {
        AgentId = agentId;
        AgentName = agentName;
        AgentTitle = agentTitle;
    }
}
