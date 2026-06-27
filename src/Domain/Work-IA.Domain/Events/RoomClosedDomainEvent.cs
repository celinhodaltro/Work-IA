using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Communication;

namespace Work_IA.Domain.Events;

public sealed record RoomClosedDomainEvent : DomainEvent
{
    public ConversationRoomId RoomId { get; }
    public string Topic { get; }

    public RoomClosedDomainEvent(ConversationRoomId roomId, string topic)
    {
        RoomId = roomId;
        Topic = topic;
    }
}
