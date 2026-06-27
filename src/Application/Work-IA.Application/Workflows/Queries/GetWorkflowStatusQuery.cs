using MediatR;
using Work_IA.Domain.Workflows;

namespace Work_IA.Application.Workflows.Queries;

public sealed record GetWorkflowStatusQuery(WorkflowId WorkflowId) : IRequest<WorkflowInstance?>;
