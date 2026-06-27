namespace Work_IA.Domain.Workflows;

public sealed class WorkflowStep
{
    public string Name { get; }
    public string ActionType { get; }
    public Dictionary<string, string> Parameters { get; }
    public int Order { get; }
    public string? DependsOnStep { get; }

    public WorkflowStep(string name, string actionType, Dictionary<string, string> parameters, int order, string? dependsOnStep = null)
    {
        Name = name;
        ActionType = actionType;
        Parameters = parameters;
        Order = order;
        DependsOnStep = dependsOnStep;
    }
}
