using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class ChiefReviewerAgent : AgentBase
{
    public ChiefReviewerAgent(IEventBus eventBus, IMediator mediator, ILogger<ChiefReviewerAgent> logger)
        : base("Chief Reviewer", AgentRole.ChiefReviewer, eventBus, mediator, logger)
    {
        RegisterObservation("CodeReviewRequested", ObservationPriority.Critical);
        RegisterObservation("ArchitectureDecisionMade", ObservationPriority.High);
        RegisterObservation("PullRequestOpened", ObservationPriority.High);
    }
    
    protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Chief Reviewer: Review task {Task}", task.Title);
        
        if (task.Title.Contains("block", StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogWarning("Chief Reviewer: Blocking delivery due to: {Task}", task.Title);
        }
        
        return Task.FromResult(TaskResult.Ok($"Review completed: {task.Title}"));
    }
    
    public bool CanBlockDelivery(string reason)
    {
        Logger.LogWarning("Chief Reviewer: Delivery blocked. Reason: {Reason}", reason);
        return true;
    }
}
