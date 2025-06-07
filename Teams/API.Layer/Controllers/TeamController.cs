using Teams.APP.Layer.CQRS.Queries;
using Teams.API.Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using MediatR;
namespace Teams.API.Layer.Controllers;

public class TeamController : ControllerBase
{
    private readonly IMediator mediator;
    public TeamController(IMediator mediator)
    {
        this.mediator = mediator;
    }
    [HttpGet("teams")]
    public async Task<ActionResult<TeamDto>> GetAllTeams()
    {
        var query = new GetAllTeamsQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }

    // [HttpGet("{id}")]
    // public async Task<ActionResult<TeamDto>> GetTeam(Guid id)
    // {
    //     var query = new GetTeamQuery(id);
    //     var result = await mediator.Send(query);
    //     return Ok(result);
    // }

    // [HttpPost]
    // public async Task<ActionResult> CreateTeam([FromBody] CreateTeamCommand command)
    // {
    //     await mediator.Send(command);
    //     return CreatedAtAction(nameof(GetTeam), new { id = command.Id }, command);
    // }
}
