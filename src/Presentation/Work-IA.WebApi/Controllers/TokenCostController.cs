using MediatR;
using Microsoft.AspNetCore.Mvc;
using Work_IA.Application.Agents.Queries;
using Work_IA.Domain.Agents;

namespace Work_IA.WebApi.Controllers;

[ApiController]
[Route("api/tokens")]
public sealed class TokenCostController : ControllerBase
{
    private readonly IMediator _mediator;

    public TokenCostController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("agents")]
    public async Task<IActionResult> GetAllCosts([FromQuery] int days = 30)
    {
        var costs = await _mediator.Send(new GetAllAgentsCostQuery(days));
        return Ok(costs);
    }

    [HttpGet("agents/{id}")]
    public async Task<IActionResult> GetAgentCost(Guid id, [FromQuery] int days = 30)
    {
        var cost = await _mediator.Send(new GetAgentCostQuery(new AgentId(id), days));
        return cost is null ? NotFound() : Ok(cost);
    }
}
