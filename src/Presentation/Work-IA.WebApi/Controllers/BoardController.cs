using MediatR;
using Microsoft.AspNetCore.Mvc;
using Work_IA.Application.Board.Commands;

namespace Work_IA.WebApi.Controllers;

[ApiController]
[Route("api/board")]
public sealed class BoardController : ControllerBase
{
    private readonly IMediator _mediator;

    public BoardController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskCommand command)
        => Ok(await _mediator.Send(command));

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _mediator.Send(new Application.Board.Queries.GetBoardQuery()));
}
