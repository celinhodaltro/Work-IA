using Work_IA.Domain.Abstractions;
using Work_IA.Domain.Events;

namespace Work_IA.Domain.Workflows;

public sealed class WorkflowInstance : AggregateRoot<WorkflowId>
{
    public string WorkflowName { get; private set; }
    public string TriggerEventId { get; private set; }
    public WorkflowInstanceState State { get; private set; }
    public int CurrentStepIndex { get; private set; }
    public List<WorkflowExecutionLog> ExecutionLog { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Error { get; private set; }
    
    private WorkflowInstance() : base() { }
    
    private WorkflowInstance(string workflowName, string triggerEventId) : base(WorkflowId.New())
    {
        WorkflowName = workflowName;
        TriggerEventId = triggerEventId;
        State = WorkflowInstanceState.Pending;
        CurrentStepIndex = 0;
        ExecutionLog = [];
    }
    
    public static WorkflowInstance Create(string workflowName, string triggerEventId)
    {
        return new WorkflowInstance(workflowName, triggerEventId);
    }
    
    public void Start()
    {
        State = WorkflowInstanceState.Running;
        StartedAt = DateTime.UtcNow;
        RaiseDomainEvent(new WorkflowStartedDomainEvent(Id, WorkflowName, TriggerEventId));
        IncrementVersion();
    }
    
    public void AdvanceStep(string stepName, bool success, string? output = null)
    {
        ExecutionLog.Add(new WorkflowExecutionLog(stepName, success, output, DateTime.UtcNow));
        
        if (success)
        {
            CurrentStepIndex++;
            RaiseDomainEvent(new WorkflowStepCompletedDomainEvent(Id, stepName, output));
        }
        else
        {
            State = WorkflowInstanceState.Failed;
            Error = output;
            CompletedAt = DateTime.UtcNow;
            RaiseDomainEvent(new WorkflowFailedDomainEvent(Id, stepName, output ?? "Unknown error"));
        }
        IncrementVersion();
    }
    
    public void Complete()
    {
        State = WorkflowInstanceState.Completed;
        CompletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new WorkflowCompletedDomainEvent(Id));
        IncrementVersion();
    }
    
    public void Fail(string error)
    {
        State = WorkflowInstanceState.Failed;
        Error = error;
        CompletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new WorkflowFailedDomainEvent(Id, CurrentStepIndex.ToString(), error));
        IncrementVersion();
    }
    
    public void Cancel()
    {
        State = WorkflowInstanceState.Cancelled;
        CompletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new WorkflowCancelledDomainEvent(Id));
        IncrementVersion();
    }
}
