using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class AgentMessage : ValueObject
{
    public AgentId From { get; }
    public AgentId To { get; }
    public string Content { get; }
    public DateTime SentAt { get; }
    public MessageType Type { get; }

    public AgentMessage(AgentId from, AgentId to, string content, MessageType type)
    {
        From = from;
        To = to;
        Content = content;
        SentAt = DateTime.UtcNow;
        Type = type;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return From;
        yield return To;
        yield return SentAt;
    }
}
