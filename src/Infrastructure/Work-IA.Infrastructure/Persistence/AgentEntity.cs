namespace Work_IA.Infrastructure.Persistence;

public sealed class AgentEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int CareerLevel { get; set; }
    public int Status { get; set; }
    public int ExperiencePoints { get; set; }
    public string? SkillsJson { get; set; }
    public Guid? MentorId { get; set; }
    public Guid? RoleId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastPromotionAt { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public bool IsHead { get; set; }
    public bool CanRead { get; set; } = true;
    public bool CanWrite { get; set; } = true;
    public bool CanDelegate { get; set; }
    public Guid? ReportsTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
