namespace Work_IA.Infrastructure.Persistence;

public sealed class MemoryEntryEntity
{
    public Guid Id { get; set; }
    public Guid MemoryId { get; set; }
    public int Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string TagsJson { get; set; } = "[]";
    public int Score { get; set; }
    public Guid? AgentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
