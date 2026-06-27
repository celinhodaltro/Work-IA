using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;
using Work_IA.Domain.Events;

namespace Work_IA.Domain.Communication;

public sealed class ConversationRoom : AggregateRoot<ConversationRoomId>
{
    private readonly List<AgentMessage> _messages = [];
    private readonly List<AgentId> _participants = [];

    public string Topic { get; private set; }
    public AgentId? Moderator { get; private set; }
    public ConversationStatus Status { get; private set; }
    public IReadOnlyList<AgentMessage> Messages => _messages.AsReadOnly();
    public IReadOnlyList<AgentId> Participants => _participants.AsReadOnly();
    public DateTime? ClosedAt { get; private set; }

    private ConversationRoom(string topic, List<AgentId> participants, AgentId? moderator)
        : base(ConversationRoomId.New())
    {
        Topic = topic;
        _participants = participants;
        Moderator = moderator;
        Status = ConversationStatus.Active;
    }

    public static ConversationRoom Create(string topic, List<AgentId> participants, AgentId? moderator = null)
    {
        var room = new ConversationRoom(topic, participants, moderator);
        room.RaiseDomainEvent(new RoomCreatedDomainEvent(room.Id, topic, participants));
        return room;
    }

    public void AddMessage(AgentMessage message)
    {
        if (Status != ConversationStatus.Active)
            throw new InvalidOperationException("Room is not active");

        _messages.Add(message);
    }

    public void AddParticipant(AgentId agentId)
    {
        if (!_participants.Contains(agentId))
        {
            _participants.Add(agentId);
        }
    }

    public void RemoveParticipant(AgentId agentId)
    {
        _participants.Remove(agentId);
    }

    public void Close()
    {
        Status = ConversationStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        RaiseDomainEvent(new RoomClosedDomainEvent(Id, Topic));
    }
}

public enum ConversationStatus
{
    Active,
    Closed,
    Archived
}
