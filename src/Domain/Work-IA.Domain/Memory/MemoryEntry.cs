using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Agents;

namespace Work_IA.Domain.Memory;

public sealed class MemoryEntry : AggregateRoot<MemoryId>
{
    public MemoryType Type { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public List<string> Tags { get; private set; }
    public int Score { get; private set; }
    public AgentId? AgentId { get; private set; }

    private MemoryEntry(
        MemoryId id,
        MemoryType type,
        string title,
        string content,
        List<string> tags,
        int score,
        AgentId? agentId) : base(id)
    {
        Type = type;
        Title = title;
        Content = content;
        Tags = tags;
        Score = score;
        AgentId = agentId;
    }

    public static MemoryEntry Create(
        MemoryType type,
        string title,
        string content,
        List<string>? tags = null,
        int score = 10,
        AgentId? agentId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (score < 0 || score > 10)
            throw new ArgumentException("Score must be between 0 and 10", nameof(score));

        return new MemoryEntry(
            MemoryId.New(),
            type,
            title,
            content ?? string.Empty,
            tags ?? [],
            score,
            agentId);
    }

    public void UpdateScore(int newScore)
    {
        if (newScore < 0 || newScore > 10)
            throw new ArgumentException("Score must be between 0 and 10", nameof(newScore));

        Score = newScore;
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
            Tags.Add(tag);
    }
}
