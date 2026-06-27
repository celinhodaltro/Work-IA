using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Workflows;

namespace Work_IA.Domain.Events;

public sealed record WorkflowStartedDomainEvent : DomainEvent
{
    public WorkflowId WorkflowId { get; }
    public string WorkflowName { get; }
    public string TriggerEventId { get; }

    public WorkflowStartedDomainEvent(WorkflowId workflowId, string workflowName, string triggerEventId)
    {
        WorkflowId = workflowId;
        WorkflowName = workflowName;
        TriggerEventId = triggerEventId;
    }
}
