using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class SpecialistAgent : AgentBase
{
    public SpecialistAgent(IEventBus eventBus, IMediator mediator, ILogger<SpecialistAgent> logger)
        : base("Specialist", AgentRole.Specialist, eventBus, mediator, logger)
    {
        RegisterObservation("TaskAssigned", ObservationPriority.High);
    }

    protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Specialist processing: {Task}", task.Title);
        return Task.FromResult(TaskResult.Ok($"Specialist task completed: {task.Title}"));
    }
}
