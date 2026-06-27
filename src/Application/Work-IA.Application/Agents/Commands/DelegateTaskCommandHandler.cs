using MediatR;
using Work_IA.Application.Agents;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Commands;

public sealed class DelegateTaskCommandHandler : IRequestHandler<DelegateTaskCommand, TaskResult>
{
    private readonly AgentRegistry _registry;

    public DelegateTaskCommandHandler(AgentRegistry registry)
    {
        _registry = registry;
    }

    public async Task<TaskResult> Handle(DelegateTaskCommand request, CancellationToken cancellationToken)
    {
        var candidates = _registry.GetByRole(request.TargetRole)
            .Where(a => a.Status == AgentStatus.Running)
            .ToList();

        if (candidates.Count == 0)
            return TaskResult.Fail($"No available agents found for role {request.TargetRole}");

        var target = candidates[Random.Shared.Next(candidates.Count)];

        var task = AgentTask.Create(request.Title, request.Description, target.AgentId, null, request.Priority);

        return await target.ExecuteTaskAsync(task, cancellationToken);
    }
}
