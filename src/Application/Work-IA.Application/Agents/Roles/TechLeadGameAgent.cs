using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class TechLeadGameAgent : AgentBase
{
    public TechLeadGameAgent(IEventBus eventBus, IMediator mediator, ILogger<TechLeadGameAgent> logger)
        : base("Tech Lead Game", AgentRole.TechLeadGame, eventBus, mediator, logger)
    {
        RegisterObservation("BuildFailed", ObservationPriority.High);
    }

    protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Tech Lead Game processing: {Task}", task.Title);
        return Task.FromResult(TaskResult.Ok($"Game task completed: {task.Title}"));
    }
}
