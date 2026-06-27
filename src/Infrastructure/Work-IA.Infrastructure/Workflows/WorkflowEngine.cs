using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workflows;

namespace Work_IA.Infrastructure.Workflows;

public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly ConcurrentDictionary<string, WorkflowDefinition> _definitions = new();
    private readonly ConcurrentDictionary<Guid, WorkflowInstance> _instances = new();
    private readonly IEventBus _eventBus;
    private readonly ILogger<WorkflowEngine> _logger;
    
    public WorkflowEngine(IEventBus eventBus, ILogger<WorkflowEngine> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }
    
    public Task RegisterWorkflowAsync(WorkflowDefinition workflow, CancellationToken cancellationToken = default)
    {
        _definitions[workflow.Name] = workflow;
        _logger.LogInformation("Workflow registered: {Name}", workflow.Name);
        return Task.CompletedTask;
    }
    
    public async Task<WorkflowInstance> ExecuteWorkflowAsync(string workflowName, string triggerEventId, Dictionary<string, string>? context = null, CancellationToken cancellationToken = default)
    {
        if (!_definitions.TryGetValue(workflowName, out var definition))
        {
            throw new ArgumentException($"Workflow '{workflowName}' not found");
        }
        
        var instance = WorkflowInstance.Create(workflowName, triggerEventId);
        _instances[instance.Id.Value] = instance;
        instance.Start();
        
        _logger.LogInformation("Workflow '{Name}' started. Instance: {InstanceId}", workflowName, instance.Id.Value);
        
        foreach (var step in definition.Steps.OrderBy(s => s.Order))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                instance.Cancel();
                return instance;
            }
            
            try
            {
                var result = await ExecuteStepAsync(step, instance, context, cancellationToken);
                instance.AdvanceStep(step.Name, true, result);
                _logger.LogInformation("Step '{Step}' completed", step.Name);
            }
            catch (Exception ex)
            {
                instance.AdvanceStep(step.Name, false, ex.Message);
                _logger.LogError(ex, "Step '{Step}' failed", step.Name);
                return instance;
            }
        }
        
        instance.Complete();
        _logger.LogInformation("Workflow '{Name}' completed successfully", workflowName);
        return instance;
    }
    
    public Task<WorkflowInstance?> GetWorkflowStatusAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        _instances.TryGetValue(instanceId, out var instance);
        return Task.FromResult(instance);
    }
    
    public Task<IReadOnlyList<WorkflowInstance>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        var active = _instances.Values
            .Where(i => i.State == WorkflowInstanceState.Running || i.State == WorkflowInstanceState.Pending)
            .ToList();
        
        return Task.FromResult<IReadOnlyList<WorkflowInstance>>(active);
    }
    
    public Task CancelWorkflowAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        if (_instances.TryGetValue(instanceId, out var instance))
        {
            instance.Cancel();
        }
        return Task.CompletedTask;
    }
    
    private async Task<string> ExecuteStepAsync(WorkflowStep step, WorkflowInstance instance, Dictionary<string, string>? context, CancellationToken cancellationToken)
    {
        switch (step.ActionType)
        {
            case "NotifyAgent":
                var message = step.Parameters.GetValueOrDefault("Message", "Workflow notification");
                var targetRole = step.Parameters.GetValueOrDefault("TargetRole", "TechLeadBackend");
                
                await _eventBus.PublishAsync(new WorkflowNotificationEvent
                {
                    WorkflowInstanceId = instance.Id.Value.ToString(),
                    WorkflowName = instance.WorkflowName,
                    TargetRole = targetRole,
                    Message = message,
                    StepName = step.Name
                }, cancellationToken);
                
                return $"Notification published for role: {targetRole}";
                
            case "DelegateTask":
                var taskRole = step.Parameters.GetValueOrDefault("TargetRole", "TechLeadBackend");
                var taskTitle = step.Parameters.GetValueOrDefault("Title", "Workflow task");
                
                await _eventBus.PublishAsync(new WorkflowTaskDelegationEvent
                {
                    WorkflowInstanceId = instance.Id.Value.ToString(),
                    WorkflowName = instance.WorkflowName,
                    TargetRole = taskRole,
                    Title = taskTitle,
                    Description = $"Auto-generated by workflow: {step.Name}",
                    StepName = step.Name
                }, cancellationToken);
                
                return $"Task delegation published for role: {taskRole}";
                
            case "RecordDecision":
                var decision = step.Parameters.GetValueOrDefault("Decision", "No decision recorded");
                
                await _eventBus.PublishAsync(new WorkflowDecisionRecordedEvent
                {
                    WorkflowInstanceId = instance.Id.Value.ToString(),
                    WorkflowName = instance.WorkflowName,
                    Decision = decision,
                    StepName = step.Name
                }, cancellationToken);
                
                return $"Decision recorded: {decision}";
                
            default:
                _logger.LogWarning("Unknown action type: {Action}", step.ActionType);
                return $"Unknown action: {step.ActionType}";
        }
    }
}
