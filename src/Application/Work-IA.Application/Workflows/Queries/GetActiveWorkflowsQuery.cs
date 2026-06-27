using MediatR;
using Work_IA.Domain.Workflows;

namespace Work_IA.Application.Workflows.Queries;

public sealed record GetActiveWorkflowsQuery : IRequest<IReadOnlyList<WorkflowInstance>>;
