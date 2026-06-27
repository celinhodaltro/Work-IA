using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class AgentTask : Entity<Guid>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public AgentId AssignedTo { get; private set; }
    public AgentId? DelegatedBy { get; private set; }
    public AgentTaskStatus TaskStatus { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    private AgentTask(
        string title,
        string description,
        AgentId assignedTo,
        AgentId? delegatedBy,
        TaskPriority priority) : base(Guid.NewGuid())
    {
        Title = title;
        Description = description;
        AssignedTo = assignedTo;
        DelegatedBy = delegatedBy;
        TaskStatus = AgentTaskStatus.Pending;
        Priority = priority;
    }
    
    public static AgentTask Create(string title, string description, AgentId assignedTo, AgentId? delegatedBy = null, TaskPriority priority = TaskPriority.Normal)
    {
        return new AgentTask(title, description, assignedTo, delegatedBy, priority);
    }
    
    public void Start()
    {
        TaskStatus = AgentTaskStatus.InProgress;
    }
    
    public void Complete()
    {
        TaskStatus = AgentTaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void Fail()
    {
        TaskStatus = AgentTaskStatus.Failed;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void Cancel()
    {
        TaskStatus = AgentTaskStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
}

public enum AgentTaskStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

public enum TaskPriority
{
    Low,
    Normal,
    High,
    Critical
}
