using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Agents;

namespace Work_IA.Application.Agents.Roles;

public sealed class AuditLeadAgent : AgentBase
{
    public AuditLeadAgent(IEventBus eventBus, IMediator mediator, ILogger<AuditLeadAgent> logger)
        : base("Audit Lead", AgentRole.AuditLead, eventBus, mediator, logger)
    {
        RegisterObservation("SprintCompleted", ObservationPriority.High);
        RegisterObservation("AgentFailure", ObservationPriority.Critical);
        RegisterObservation("ArchitectureDecisionMade", ObservationPriority.High);
        RegisterObservation("MemoryRecorded", ObservationPriority.Medium);
    }
    
    protected override async Task<TaskResult> ProcessTaskAsync(AgentTask task, CancellationToken cancellationToken)
    {
        Logger.LogInformation("Audit Lead: Auditing {Task}", task.Title);
        
        var lessons = new List<string>
        {
            "What went wrong?",
            "Why did it go wrong?",
            "How to prevent it again?",
            "What worked well?",
            "What should become standard?"
        };
        
        foreach (var lesson in lessons)
        {
            Logger.LogInformation("Audit question: {Lesson}", lesson);
        }
        
        return TaskResult.Ok($"Audit completed: {task.Title}");
    }
}
