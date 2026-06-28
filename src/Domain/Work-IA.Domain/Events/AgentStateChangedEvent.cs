using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Events;

public sealed record AgentStateChangedEvent : DomainEvent
{
    public AgentId AgentId { get; }
    public string Name { get; }
    public string Title { get; }
    public string Status { get; }
    public string Emotion { get; }
    public string Action { get; }
    public string? ConversationTopic { get; }
    public float PositionX { get; }
    public float PositionY { get; }

    public AgentStateChangedEvent(
        AgentId agentId, string name, string title,
        string status, string emotion, string action,
        string? conversationTopic, float positionX, float positionY)
    {
        AgentId = agentId;
        Name = name;
        Title = title;
        Status = status;
        Emotion = emotion;
        Action = action;
        ConversationTopic = conversationTopic;
        PositionX = positionX;
        PositionY = positionY;
    }
}
