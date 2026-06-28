using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Agents;

public sealed class BoardTask : Entity<Guid>
{
    public string Title { get; private set; }
    public string Description { get; private set; }
    public AgentId? AssignedTo { get; private set; }
    public BoardColumn Column { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private BoardTask() : base(Guid.NewGuid()) { }

    public static BoardTask Create(string title, string description, TaskPriority priority = TaskPriority.Normal)
    {
        return new BoardTask
        {
            Title = title,
            Description = description,
            Column = BoardColumn.Pending,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AssignTo(AgentId agentId)
    {
        AssignedTo = agentId;
        Column = BoardColumn.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void MoveTo(BoardColumn column)
    {
        Column = column;
        if (column == BoardColumn.InProgress) StartedAt ??= DateTime.UtcNow;
        if (column == BoardColumn.Completed) CompletedAt = DateTime.UtcNow;
    }

    public void SetPriority(TaskPriority priority) => Priority = priority;
}

public enum BoardColumn { Pending, InProgress, Review, Completed }
