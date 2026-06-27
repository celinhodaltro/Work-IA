using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class CeoAgent : AgentBase, ICeoAgent
{
    public CeoAgent(IEventBus eventBus, IMediator mediator, ILogger<CeoAgent> logger)
        : base("CEO", AgentRole.Ceo, eventBus, mediator, logger)
    {
        RegisterObservation("SprintCompleted", ObservationPriority.Low);
        RegisterObservation("ReleaseCompleted", ObservationPriority.High);
        RegisterObservation("MajorArchitectureChange", ObservationPriority.High);
        RegisterObservation("ProjectMilestoneReached", ObservationPriority.Critical);
    }
    
    protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("CEO reviewing strategic impact: {Task}", task.Title);
        return Task.FromResult(TaskResult.Ok($"Strategic review completed: {task.Title}"));
    }
    
    public async Task ReceiveWeeklySummaryAsync(string summary)
    {
        Logger.LogInformation("CEO Weekly Summary:\n{Summary}", summary);
        await Task.CompletedTask;
    }
}
