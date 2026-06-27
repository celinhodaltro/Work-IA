using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class HeadOfEngineeringAgent : AgentBase
{
    public HeadOfEngineeringAgent(IEventBus eventBus, IMediator mediator, ILogger<HeadOfEngineeringAgent> logger)
        : base("Head of Engineering", AgentRole.HeadOfEngineering, eventBus, mediator, logger)
    {
        RegisterObservation("AgentTaskDelegated", ObservationPriority.High);
        RegisterObservation("AgentFailure", ObservationPriority.Critical);
        RegisterObservation("BuildFailed", ObservationPriority.High);
        RegisterObservation("DeploymentStarted", ObservationPriority.Medium);
        RegisterObservation("WorkflowCompleted", ObservationPriority.Low);
        RegisterObservation("ArchitectureDecisionMade", ObservationPriority.Medium);
    }
    
    protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Head of Engineering coordinating: {Task}", task.Title);
        return Task.FromResult(TaskResult.Ok($"Head approved and coordinated: {task.Title}"));
    }
}
