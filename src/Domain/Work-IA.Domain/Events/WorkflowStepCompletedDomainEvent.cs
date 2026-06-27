using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Workflows;

namespace Work_IA.Domain.Events;

public sealed record WorkflowStepCompletedDomainEvent : DomainEvent
{
    public WorkflowId WorkflowId { get; }
    public string StepName { get; }
    public string? Output { get; }

    public WorkflowStepCompletedDomainEvent(WorkflowId workflowId, string stepName, string? output)
    {
        WorkflowId = workflowId;
        StepName = stepName;
        Output = output;
    }
}
