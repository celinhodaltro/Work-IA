namespace Work_IA.Domain.Workflows;

public sealed class WorkflowDefinition
{
    public WorkflowId Id { get; init; }
    public string Name { get; init; }
    public string TriggerEvent { get; init; }
    public List<WorkflowStep> Steps { get; init; } = [];
    public bool IsActive { get; set; } = true;

    private WorkflowDefinition(WorkflowId id, string name, string triggerEvent, List<WorkflowStep> steps)
    {
        Id = id;
        Name = name;
        TriggerEvent = triggerEvent;
        Steps = steps;
    }

    public static WorkflowDefinition Create(string name, string triggerEvent, List<WorkflowStep> steps)
    {
        return new WorkflowDefinition(WorkflowId.New(), name, triggerEvent, steps);
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
