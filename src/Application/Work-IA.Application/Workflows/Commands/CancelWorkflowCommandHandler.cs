using MediatR;
using Work_IA.Application.Common.Exceptions;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workflows;

namespace Work_IA.Application.Workflows.Commands;

public sealed class CancelWorkflowCommandHandler : IRequestHandler<CancelWorkflowCommand>
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IWorkflowRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelWorkflowCommandHandler(
        IWorkflowEngine workflowEngine,
        IWorkflowRepository repository,
        IUnitOfWork unitOfWork)
    {
        _workflowEngine = workflowEngine;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelWorkflowCommand request, CancellationToken cancellationToken)
    {
        var instance = await _repository.GetByIdAsync(request.WorkflowId, cancellationToken);

        if (instance is null)
            throw new NotFoundException(nameof(WorkflowInstance), request.WorkflowId.Value);

        instance.Cancel();

        await _repository.UpdateAsync(instance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
