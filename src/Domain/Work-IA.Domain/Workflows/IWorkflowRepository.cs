using Work_IA.Domain.Abstractions;

namespace Work_IA.Domain.Workflows;

public interface IWorkflowRepository : IRepository<WorkflowInstance, WorkflowId>
{
    Task<IReadOnlyList<WorkflowInstance>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowInstance>> GetByStatusAsync(WorkflowInstanceState status, CancellationToken cancellationToken = default);
}
