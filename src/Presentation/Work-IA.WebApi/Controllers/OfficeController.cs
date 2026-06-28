using MediatR;
using Microsoft.AspNetCore.Mvc;
using Work_IA.Application.Agents.Commands;
using Work_IA.Application.Board.Queries;
using Work_IA.Domain.Agents;
using Work_IA.Application.Agents.Queries;
using Work_IA.Application.Services;
using Work_IA.Application.Agents;

namespace Work_IA.WebApi.Controllers;

[ApiController]
[Route("api/office")]
public sealed class OfficeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly AgentRegistry _registry;

    public OfficeController(IMediator mediator, AgentRegistry registry)
    {
        _mediator = mediator;
        _registry = registry;
    }

    [HttpPost("meetings")]
    public async Task<IActionResult> ScheduleMeeting([FromBody] ScheduleMeetingCommand command)
        => Ok(await _mediator.Send(command));

    [HttpGet("meetings")]
    public IActionResult GetActiveMeetings()
    {
        var agents = _registry.GetAll().Where(a => a.Status == AgentStatus.Running).ToList();
        return Ok(new { onlineCount = agents.Count, agents = agents.Select(a => new { a.AgentId, a.Name }) });
    }

    [HttpPost("agents/{id}/fire")]
    public IActionResult FireAgent(Guid id)
    {
        var agent = _registry.Get(new AgentId(id));
        if (agent is null) return NotFound();
        _registry.Unregister(new AgentId(id));
        return Ok(new { message = "Agent fired", agentId = id });
    }
}
