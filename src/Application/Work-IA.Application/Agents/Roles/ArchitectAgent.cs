using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class ArchitectAgent : AgentBase
{
    public ArchitectAgent(IEventBus eventBus, IMediator mediator, ILogger<ArchitectAgent> logger)
        : base("Architect", AgentRole.Architect, eventBus, mediator, logger)
    {
        RegisterObservation("FileModified", ObservationPriority.High, e => e?.ToString()?.Contains("IInterface") == true);
        RegisterObservation("ArchitectureDecisionProposed", ObservationPriority.Critical);
        RegisterObservation("PullRequestOpened", ObservationPriority.High);
    }
    
    protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Architect reviewing architecture: {Task}", task.Title);
        return Task.FromResult(TaskResult.Ok($"Architecture reviewed: {task.Title}"));
    }
    
    public bool HasVetoPower => true;
}
