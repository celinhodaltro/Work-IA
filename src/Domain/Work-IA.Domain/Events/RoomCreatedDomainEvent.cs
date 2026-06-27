using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Communication;

namespace Work_IA.Domain.Events;

public sealed record RoomCreatedDomainEvent : DomainEvent
{
    public ConversationRoomId RoomId { get; }
    public string Topic { get; }
    public List<AgentId> Participants { get; }

    public RoomCreatedDomainEvent(ConversationRoomId roomId, string topic, List<AgentId> participants)
    {
        RoomId = roomId;
        Topic = topic;
        Participants = participants;
    }
}
