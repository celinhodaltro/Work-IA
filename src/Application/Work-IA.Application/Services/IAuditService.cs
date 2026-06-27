namespace Work_IA.Application.Services;

public interface IAuditService
{
    void Record(string userId, string action, string resourceType, string resourceId, string details);
    IReadOnlyList<AuditEntry> GetRecent(int count = 50);
}

public sealed class AuditEntry
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
