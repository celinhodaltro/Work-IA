using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workflows;

namespace Work_IA.Application.Workflows.Queries;

public sealed class GetWorkflowStatusQueryHandler : IRequestHandler<GetWorkflowStatusQuery, WorkflowInstance?>
{
    private readonly IWorkflowEngine _workflowEngine;

    public GetWorkflowStatusQueryHandler(IWorkflowEngine workflowEngine)
    {
        _workflowEngine = workflowEngine;
    }

    public async Task<WorkflowInstance?> Handle(GetWorkflowStatusQuery request, CancellationToken cancellationToken)
    {
        return await _workflowEngine.GetWorkflowStatusAsync(request.WorkflowId.Value, cancellationToken);
    }
}
