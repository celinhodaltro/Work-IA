using MediatR;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class TechLeadFrontendAgent : AgentBase
{
    public TechLeadFrontendAgent(IEventBus eventBus, IMediator mediator, ILogger<TechLeadFrontendAgent> logger)
        : base("Tech Lead Frontend", AgentRole.TechLeadFrontend, eventBus, mediator, logger)
    {
        RegisterObservation("FileModified", ObservationPriority.High, e => e?.ToString()?.Contains(".razor") == true);
        RegisterObservation("FileModified", ObservationPriority.High, e => e?.ToString()?.Contains(".css") == true);
        RegisterObservation("FileModified", ObservationPriority.High, e => e?.ToString()?.Contains(".js") == true);
        RegisterObservation("BuildFailed", ObservationPriority.High);
    }
    
    protected override Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Tech Lead Frontend processing: {Task}", task.Title);
        return Task.FromResult(TaskResult.Ok($"Frontend task completed: {task.Title}"));
    }
}
