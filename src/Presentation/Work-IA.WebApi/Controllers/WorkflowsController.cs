using Microsoft.AspNetCore.Mvc;
using Work_IA.Application.Common.Interfaces;
using Work_IA.Domain.Workflows;

namespace Work_IA.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WorkflowsController : ControllerBase
{
    private readonly IWorkflowEngine _engine;

    public WorkflowsController(IWorkflowEngine engine)
    {
        _engine = engine;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] WorkflowDefinition definition)
    {
        await _engine.RegisterWorkflowAsync(definition);
        return Ok(new { message = "Workflow registered" });
    }

    [HttpPost("execute/{name}")]
    public async Task<IActionResult> Execute(string name, [FromQuery] string eventId)
    {
        var instance = await _engine.ExecuteWorkflowAsync(name, eventId);
        return Ok(instance);
    }

    [HttpGet("{instanceId}")]
    public async Task<IActionResult> GetStatus(Guid instanceId)
    {
        var instance = await _engine.GetWorkflowStatusAsync(instanceId);
        if (instance is null)
            return NotFound();
        return Ok(instance);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var active = await _engine.GetActiveWorkflowsAsync();
        return Ok(active);
    }

    [HttpPost("{instanceId}/cancel")]
    public async Task<IActionResult> Cancel(Guid instanceId)
    {
        await _engine.CancelWorkflowAsync(instanceId);
        return Ok(new { message = "Workflow cancelled" });
    }
}
