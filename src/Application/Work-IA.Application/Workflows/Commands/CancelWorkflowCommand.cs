using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Workflows;

namespace Work_IA.Application.Workflows.Commands;

public sealed record CancelWorkflowCommand(WorkflowId WorkflowId) : IRequest, IRequiresUnitOfWork;
