using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workflows;

namespace Work_IA.Infrastructure.BackgroundServices;

public sealed class WorkflowStartupService : BackgroundService
{
    private readonly IWorkflowEngine _engine;
    private readonly ILogger<WorkflowStartupService> _logger;

    public WorkflowStartupService(IWorkflowEngine engine, ILogger<WorkflowStartupService> logger)
    {
        _engine = engine;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Registering default workflows...");

        var fileChangeWorkflow = WorkflowDefinition.Create(
            "FileChangeNotification",
            "FileModified",
            new List<WorkflowStep>
            {
                new("ClassifyChange", "NotifyAgent", new() { ["TargetRole"] = "TechLeadBackend", ["Message"] = "File change detected" }, 1),
                new("AssessImpact", "DelegateTask", new() { ["TargetRole"] = "Architect", ["Title"] = "Assess impact of file change" }, 2),
                new("RecordAction", "RecordDecision", new() { ["Decision"] = "File change assessed" }, 3)
            });

        await _engine.RegisterWorkflowAsync(fileChangeWorkflow, stoppingToken);

        var testFailureWorkflow = WorkflowDefinition.Create(
            "TestFailureEscalation",
            "TestFailed",
            new List<WorkflowStep>
            {
                new("AnalyzeFailure", "NotifyAgent", new() { ["TargetRole"] = "TestLead", ["Message"] = "Test failure detected" }, 1),
                new("CreateBugTask", "DelegateTask", new() { ["TargetRole"] = "TechLeadBackend", ["Title"] = "Investigate and fix test failure" }, 2),
                new("TrackFix", "RecordDecision", new() { ["Decision"] = "Bug investigation created" }, 3)
            });

        await _engine.RegisterWorkflowAsync(testFailureWorkflow, stoppingToken);

        _logger.LogInformation("Default workflows registered successfully");
    }
}
