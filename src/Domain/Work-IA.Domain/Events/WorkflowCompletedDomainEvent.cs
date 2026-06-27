using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Workflows;

namespace Work_IA.Domain.Events;

public sealed record WorkflowCompletedDomainEvent : DomainEvent
{
    public WorkflowId WorkflowId { get; }

    public WorkflowCompletedDomainEvent(WorkflowId workflowId)
    {
        WorkflowId = workflowId;
    }
}
