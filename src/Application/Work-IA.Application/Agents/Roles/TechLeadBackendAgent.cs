using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class TechLeadBackendAgent : AgentBase
{
    public TechLeadBackendAgent(IEventBus eventBus, IMediator mediator, ILogger<TechLeadBackendAgent> logger)
        : base("Tech Lead Backend", AgentRole.TechLeadBackend, eventBus, mediator, logger)
    {
        RegisterObservation("FileModified", ObservationPriority.High, e => e?.ToString()?.Contains(".cs") == true);
        RegisterObservation("FileModified", ObservationPriority.Critical, e => e?.ToString()?.Contains(".csproj") == true);
        RegisterObservation("TestFailed", ObservationPriority.High);
        RegisterObservation("BuildFailed", ObservationPriority.Critical);
        RegisterObservation("GitCommit", ObservationPriority.Medium);
    }
    
    protected override async Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Tech Lead Backend processing: {Task}", task.Title);
        
        if (task.Title.Contains("review", StringComparison.OrdinalIgnoreCase))
        {
            var architect = new AgentId(Guid.NewGuid());
            Logger.LogInformation("Delegating review to architect {ArchitectId}", architect);
        }
        
        return TaskResult.Ok($"Backend task completed: {task.Title}");
    }
}
