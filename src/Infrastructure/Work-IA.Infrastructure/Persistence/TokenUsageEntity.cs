namespace Work_IA.Infrastructure.Persistence;

public sealed class TokenUsageEntity
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string ModelName { get; set; } = "";
    public int TokensIn { get; set; }
    public int TokensOut { get; set; }
    public decimal Cost { get; set; }
    public string TaskDescription { get; set; } = "";
    public DateTime Timestamp { get; set; }
}
