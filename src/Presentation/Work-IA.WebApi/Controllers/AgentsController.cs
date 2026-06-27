using MediatR;
using Microsoft.AspNetCore.Mvc;
using Work_IA.Application.Agents.Commands;
using Work_IA.Application.Agents.Queries;
using Work_IA.Domain.Agents;
using Work_IA.WebApi.Models;

namespace Work_IA.WebApi.Controllers;

[ApiController]
[Route("api/agents")]
public sealed class AgentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var agents = await _mediator.Send(new ListAgentsQuery(), ct);
        var dtos = agents.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var agent = await _mediator.Send(new GetAgentQuery(new AgentId(id)), ct);
        if (agent is null)
            return NotFound();
        return Ok(MapToDto(agent));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAgentRequest request, CancellationToken ct)
    {
        var agentId = await _mediator.Send(
            new CreateAgentCommand(request.Name, request.Title, request.RoleId), ct);
        return Ok(new { agentId = agentId.Value });
    }

    private static AgentDto MapToDto(Agent agent) => new()
    {
        AgentId = agent.AgentId.Value,
        Name = agent.Name.Value,
        Title = agent.Title.Value,
        CareerLevel = (int)agent.CareerLevel,
        Status = agent.Status.ToString(),
        ExperiencePoints = agent.ExperiencePoints,
        JoinedAt = agent.JoinedAt
    };
}
