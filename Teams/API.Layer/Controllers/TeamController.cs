using Teams.APP.Layer.CQRS.Queries;
using Teams.API.Layer.DTOs;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Teams.APP.Layer.CQRS.Commands;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
namespace Teams.API.Layer.Controllers;

[ApiController]
[Route("teams")]
public class TeamController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<TeamDto>> GetAllTeams()
    {
        var query = new GetAllTeamsQuery();
        var teams = await mediator.Send(query);
        return Ok(teams);
    }

    [HttpGet("{teamId:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDto>> GetTeam(Guid teamId)
    {
        var team = await mediator.Send(new GetTeamQuery(teamId));
        return Ok(team);
    }

    [HttpGet("manager")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TeamDto>>> GetTeamsByManagerId([FromQuery] Guid managerId, [FromQuery] bool includeMembers = false, CancellationToken cancellationToken = default)
    {
        var teams = await mediator.Send(new GetTeamsByManagerQuery(managerId, includeMembers), cancellationToken);
        return Ok(teams);
    }
    [HttpGet("member")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TeamDto>>> GetTeamsByMemberId([FromQuery] Guid memberId, [FromQuery] bool includeMembers = false, CancellationToken cancellationToken = default)
    {
        var teams = await mediator.Send(new GetTeamsByMemberQuery(memberId, includeMembers), cancellationToken);
        return Ok(teams);
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CreateTeamCommand>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamCommand command)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdTeam=await mediator.Send(command);
        return CreatedAtAction(nameof(GetTeam), new { teamId = createdTeam.Id }, createdTeam);
    }

    // [Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [HttpPut("{teamId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamRequestDto>> UpdateTeamById(Guid teamId, [FromBody] UpdateTeamCommand command, CancellationToken cancellationToken = default)
    {
        if (teamId != command.Id)
            return BadRequest("Team ID in the URL does not match the ID in the request body.");
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var team = await mediator.Send(command, cancellationToken);
        return Ok(team);
    }
    //[Authorize(Roles = "Admin")]
    [HttpDelete("{teamId:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTeam(Guid teamId)
    {
        await mediator.Send(new DeleteTeamCommand(teamId));
        return NoContent();
    }
}