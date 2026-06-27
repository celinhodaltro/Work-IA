namespace Work_IA.Infrastructure.Persistence;

public sealed class AgentEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Role { get; set; }
    public int Status { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
