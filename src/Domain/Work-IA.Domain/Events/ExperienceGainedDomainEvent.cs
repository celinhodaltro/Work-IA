using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record ExperienceGainedDomainEvent : DomainEvent
{
    public AgentId AgentId { get; }
    public int PointsGained { get; }
    public int TotalExperience { get; }

    public ExperienceGainedDomainEvent(AgentId agentId, int pointsGained, int totalExperience)
    {
        AgentId = agentId;
        PointsGained = pointsGained;
        TotalExperience = totalExperience;
    }
}
