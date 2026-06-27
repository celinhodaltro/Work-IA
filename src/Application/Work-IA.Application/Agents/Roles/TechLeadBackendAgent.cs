using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class TechLeadBackendAgent : AgentBase
{
    private readonly AgentRegistry _registry;

    public TechLeadBackendAgent(IEventBus eventBus, IMediator mediator, AgentRegistry registry, ILogger<TechLeadBackendAgent> logger)
        : base("Tech Lead Backend", AgentRole.TechLeadBackend, eventBus, mediator, logger)
    {
        _registry = registry;
        RegisterObservation("FileModified", ObservationPriority.High, e => e?.ToString()?.Contains(".cs") == true);
        RegisterObservation("FileModified", ObservationPriority.Critical, e => e?.ToString()?.Contains(".csproj") == true);
        RegisterObservation("TestFailed", ObservationPriority.High);
        RegisterObservation("BuildFailed", ObservationPriority.Critical);
        RegisterObservation("GitCommit", ObservationPriority.Medium);
    }

    protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Tech Lead Backend processing: {Task}", task.Title);

        if (task.Title.Contains("review", StringComparison.OrdinalIgnoreCase))
        {
            var architects = _registry.GetByRole(AgentRole.Architect);
            if (architects.Count > 0)
            {
                Logger.LogInformation("Delegating review to architect {ArchitectId}", architects[0].AgentId);
            }
        }

        return Task.FromResult(TaskResult.Ok($"Backend task completed: {task.Title}"));
    }
}
