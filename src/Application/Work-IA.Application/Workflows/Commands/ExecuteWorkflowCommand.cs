using MediatR;
using Work_IA.Application.Behaviors;
using Work_IA.Domain.Workflows;

namespace Work_IA.Application.Workflows.Commands;

public sealed record ExecuteWorkflowCommand(
    string WorkflowName,
    string TriggerEventId,
    Dictionary<string, string>? Context) : IRequest<WorkflowInstance>, IRequiresUnitOfWork;
