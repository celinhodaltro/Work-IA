using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record MessageReceivedDomainEvent : DomainEvent
{
    public AgentId AgentId { get; }
    public AgentId FromAgentId { get; }

    public MessageReceivedDomainEvent(AgentId agentId, AgentId fromAgentId)
    {
        AgentId = agentId;
        FromAgentId = fromAgentId;
    }
}
