namespace Work_IA.WebApi.Models;

public sealed class AgentDto
{
    public Guid AgentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int CareerLevel { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ExperiencePoints { get; set; }
    public DateTime JoinedAt { get; set; }
}
