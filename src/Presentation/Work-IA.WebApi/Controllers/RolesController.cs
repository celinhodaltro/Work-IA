using MediatR;
using Microsoft.AspNetCore.Mvc;
using Work_IA.Application.Roles.Commands;
using Work_IA.Application.Roles.Queries;

namespace Work_IA.WebApi.Controllers;

[ApiController]
[Route("api/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command)
        => Ok(await _mediator.Send(command));

    [HttpGet]
    public async Task<IActionResult> List()
        => Ok(await _mediator.Send(new ListRolesQuery()));
}
