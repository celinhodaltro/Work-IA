namespace Work_IA.Infrastructure.Persistence;

public sealed class WorkflowInstanceEntity
{
    public Guid Id { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string TriggerEventId { get; set; } = string.Empty;
    public int State { get; set; }
    public int CurrentStepIndex { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
