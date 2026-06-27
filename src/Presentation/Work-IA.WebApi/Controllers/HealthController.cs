using Microsoft.AspNetCore.Mvc;
using Work_IA.Application.Agents;

namespace Work_IA.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class HealthController : ControllerBase
{
    private readonly AgentRegistry _registry;
    
    public HealthController(AgentRegistry registry)
    {
        _registry = registry;
    }
    
    [HttpGet]
    public IActionResult Get()
    {
        var agents = _registry.GetAll();
        var onlineAgents = agents.Count(a => a.Status == Domain.Agents.AgentStatus.Running);
        
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            ActiveAgents = onlineAgents,
            TotalAgents = agents.Count,
            UptimeSeconds = (int)(DateTime.UtcNow - _startTime).TotalSeconds
        });
    }
    
    [HttpGet("ready")]
    public IActionResult Readiness()
    {
        return Ok(new { Status = "Ready" });
    }
    
    [HttpGet("live")]
    public IActionResult Liveness()
    {
        return Ok(new { Status = "Alive" });
    }
    
    private static readonly DateTime _startTime = DateTime.UtcNow;
}
