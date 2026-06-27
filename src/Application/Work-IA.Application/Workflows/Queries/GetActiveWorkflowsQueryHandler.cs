using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workflows;

namespace Work_IA.Application.Workflows.Queries;

public sealed class GetActiveWorkflowsQueryHandler : IRequestHandler<GetActiveWorkflowsQuery, IReadOnlyList<WorkflowInstance>>
{
    private readonly IWorkflowEngine _workflowEngine;

    public GetActiveWorkflowsQueryHandler(IWorkflowEngine workflowEngine)
    {
        _workflowEngine = workflowEngine;
    }

    public async Task<IReadOnlyList<WorkflowInstance>> Handle(GetActiveWorkflowsQuery request, CancellationToken cancellationToken)
    {
        return await _workflowEngine.GetActiveWorkflowsAsync(cancellationToken);
    }
}
