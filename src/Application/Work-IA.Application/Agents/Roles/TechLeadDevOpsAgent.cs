using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class TechLeadDevOpsAgent : AgentBase
{
    public TechLeadDevOpsAgent(IEventBus eventBus, IMediator mediator, ILogger<TechLeadDevOpsAgent> logger)
        : base("Tech Lead DevOps", AgentRole.TechLeadDevOps, eventBus, mediator, logger)
    {
        RegisterObservation("BuildCompleted", ObservationPriority.High);
        RegisterObservation("DeployStarted", ObservationPriority.Critical);
        RegisterObservation("DeployCompleted", ObservationPriority.High);
        RegisterObservation("InfrastructureChange", ObservationPriority.Critical);
    }
    
    protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Tech Lead DevOps processing: {Task}", task.Title);
        return Task.FromResult(TaskResult.Ok($"DevOps task completed: {task.Title}"));
    }
}
