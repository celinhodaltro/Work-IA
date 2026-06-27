using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Workflows;

public sealed class WorkflowExecutionLog : ValueObject
{
    public string StepName { get; }
    public bool Success { get; }
    public string? Output { get; }
    public DateTime Timestamp { get; }

    public WorkflowExecutionLog(string stepName, bool success, string? output, DateTime timestamp)
    {
        StepName = stepName;
        Success = success;
        Output = output;
        Timestamp = timestamp;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StepName;
        yield return Success;
        yield return Output ?? string.Empty;
        yield return Timestamp;
    }
}
