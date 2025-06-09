using Teams.APP.Layer.CQRS.Queries;
using Teams.API.Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.Models;
using System.Net.Mime;
namespace Teams.API.Layer.Controllers;

[ApiController]
public class TeamController : ControllerBase
{
    private readonly IMediator mediator;
    /// <summary>
    /// Initializes a new instance of the <see cref="TeamController"/> class.
    /// </summary>
    /// <param name="mediator">
    /// The mediator instance used to send commands and queries within the application.
    /// </param>
    public TeamController(IMediator mediator)
    {
        this.mediator = mediator;
    }
    [HttpGet("teams")]
    public async Task<ActionResult<TeamDto>> GetAllTeams()
    {
        var query = new GetAllTeamsQuery();
        var teams = await mediator.Send(query);
        return Ok(teams);
    }

    [HttpGet("team/{teamId:guid}")]
    public async Task<ActionResult<TeamDto>> GetTeam(Guid teamId)
    {
        var team = await mediator.Send(new GetTeamQuery(teamId));
        return Ok(team);
    }

    [HttpGet("manager")]
    public async Task<ActionResult<List<TeamDto>>> GetTeamsByManagerId([FromQuery] Guid managerId, [FromQuery] bool includeMembers = false, CancellationToken cancellationToken = default)
    {
        var teams = await mediator.Send(new GetTeamsByManagerQuery(managerId, includeMembers), cancellationToken);
        return Ok(teams);
    }
    [HttpGet("member")]
    public async Task<ActionResult<List<TeamDto>>> GetTeamsByMemberId([FromQuery] Guid memberId, [FromQuery] bool includeMembers = false, CancellationToken cancellationToken = default)
    {
        var teams = await mediator.Send(new GetTeamsByMemberQuery(memberId, includeMembers), cancellationToken);
        return Ok(teams);
    }

    [HttpPost("team")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CreateTeamCommand>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await mediator.Send(command);
        return CreatedAtAction(nameof(CreateTeam), new { command.Name }, command);
    }
    // [HttpDelete]
    // public async Task<ActionResult> DeleteTeam(Guid teamId)
    // {
    //     var command = new DeleteTeamCommand(teamId);
    //     await mediator.Send(command);
    //     return NoContent();
    // }
     // [HttpUpdate]
    // public async Task<ActionResult> DeleteTeam(Guid teamId)
    // {
    //     var command = new DeleteTeamCommand(teamId);
    //     await mediator.Send(command);
    //     return NoContent();
    // }
}
