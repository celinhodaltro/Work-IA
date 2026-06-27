using Work_IA.Application.Common.Interfaces;

namespace Work_IA.Infrastructure.Workflows;

public sealed record WorkflowNotificationEvent : IIntegrationEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string EventType => GetType().Name;
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required string WorkflowInstanceId { get; init; }
    public required string WorkflowName { get; init; }
    public required string TargetRole { get; init; }
    public required string Message { get; init; }
    public required string StepName { get; init; }
}

public sealed record WorkflowTaskDelegationEvent : IIntegrationEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string EventType => GetType().Name;
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required string WorkflowInstanceId { get; init; }
    public required string WorkflowName { get; init; }
    public required string TargetRole { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string StepName { get; init; }
}

public sealed record WorkflowDecisionRecordedEvent : IIntegrationEvent
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public string EventType => GetType().Name;
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public required string WorkflowInstanceId { get; init; }
    public required string WorkflowName { get; init; }
    public required string Decision { get; init; }
    public required string StepName { get; init; }
}
