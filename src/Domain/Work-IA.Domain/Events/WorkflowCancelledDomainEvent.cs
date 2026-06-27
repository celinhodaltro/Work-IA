using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Workflows;

namespace Work_IA.Domain.Events;

public sealed record WorkflowCancelledDomainEvent : DomainEvent
{
    public WorkflowId WorkflowId { get; }

    public WorkflowCancelledDomainEvent(WorkflowId workflowId)
    {
        WorkflowId = workflowId;
    }
}
