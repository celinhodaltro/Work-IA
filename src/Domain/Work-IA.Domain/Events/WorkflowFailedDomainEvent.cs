using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Workflows;

namespace Work_IA.Domain.Events;

public sealed record WorkflowFailedDomainEvent : DomainEvent
{
    public WorkflowId WorkflowId { get; }
    public string FailedStep { get; }
    public string Error { get; }

    public WorkflowFailedDomainEvent(WorkflowId workflowId, string failedStep, string error)
    {
        WorkflowId = workflowId;
        FailedStep = failedStep;
        Error = error;
    }
}
