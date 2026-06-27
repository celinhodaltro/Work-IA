using MediatR;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workflows;

namespace Work_IA.Application.Workflows.Commands;

public sealed class ExecuteWorkflowCommandHandler : IRequestHandler<ExecuteWorkflowCommand, WorkflowInstance>
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IUnitOfWork _unitOfWork;

    public ExecuteWorkflowCommandHandler(IWorkflowEngine workflowEngine, IUnitOfWork unitOfWork)
    {
        _workflowEngine = workflowEngine;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkflowInstance> Handle(ExecuteWorkflowCommand request, CancellationToken cancellationToken)
    {
        var instance = await _workflowEngine.ExecuteWorkflowAsync(
            request.WorkflowName,
            request.TriggerEventId,
            request.Context,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return instance;
    }
}
