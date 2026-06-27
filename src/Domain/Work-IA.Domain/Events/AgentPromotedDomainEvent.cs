using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record AgentPromotedDomainEvent : DomainEvent
{
    public AgentId AgentId { get; }
    public string NewTitle { get; }
    public AgentCareerLevel NewLevel { get; }

    public AgentPromotedDomainEvent(AgentId agentId, string newTitle, AgentCareerLevel newLevel)
    {
        AgentId = agentId;
        NewTitle = newTitle;
        NewLevel = newLevel;
    }
}
