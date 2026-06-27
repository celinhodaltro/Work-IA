using Work_IA.Domain.Workflows;

namespace Work_IA.Application.Common.Interfaces;

public interface IWorkflowEngine
{
    Task RegisterWorkflowAsync(WorkflowDefinition workflow, CancellationToken cancellationToken = default);
    Task<WorkflowInstance> ExecuteWorkflowAsync(string workflowName, string triggerEventId, Dictionary<string, string>? context = null, CancellationToken cancellationToken = default);
    Task<WorkflowInstance?> GetWorkflowStatusAsync(Guid instanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowInstance>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default);
    Task CancelWorkflowAsync(Guid instanceId, CancellationToken cancellationToken = default);
}
