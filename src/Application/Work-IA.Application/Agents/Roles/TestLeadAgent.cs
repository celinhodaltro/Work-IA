using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class TestLeadAgent : AgentBase
{
    public TestLeadAgent(IEventBus eventBus, IMediator mediator, ILogger<TestLeadAgent> logger)
        : base("Test Lead", AgentRole.TestLead, eventBus, mediator, logger)
    {
        RegisterObservation("TestStarted", ObservationPriority.Medium);
        RegisterObservation("TestCompleted", ObservationPriority.High);
        RegisterObservation("TestFailed", ObservationPriority.Critical);
        RegisterObservation("FileModified", ObservationPriority.Medium, e => e?.ToString()?.Contains(".cs") == true);
        RegisterObservation("BuildCompleted", ObservationPriority.High);
    }
    
    protected override async Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Test Lead processing: {Task}", task.Title);
        
        if (task.Title.Contains("regression", StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogInformation("Full regression suite scheduled");
        }
        
        return TaskResult.Ok($"Tests scheduled: {task.Title}");
    }
}
