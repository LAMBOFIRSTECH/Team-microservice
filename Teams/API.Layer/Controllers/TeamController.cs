using System.Net.Mime;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Mappings;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.CQRS.Queries;
using Teams.APP.Layer.Interfaces;

namespace Teams.API.Layer.Controllers;

[ApiController]
[Route("teams")]
public class TeamController(
    IMediator mediator,
    IValidator<CreateTeamCommand> createTeamValidator,
    IValidator<UpdateTeamCommand> updateTeamValidator,
    IEmployeeService employeeService,
    ILogger<TeamController> log
) : ControllerBase
{
    /// <summary>
    /// Retrieves all teams in the system.
    /// This endpoint allows you to get a list of all teams, including their details such as
    /// team name, members, and team manager.
    /// If no teams are found, an empty list will be returned.
    /// This endpoint is useful for administrators or managers to view all teams in the system.
    /// Authorization is not required for this endpoint, but it can be restricted to specific roles
    /// such as "Admin" or "Manager" if needed.
    /// Example usage:
    /// GET /teams
    /// This will return a list of all teams in the system.
    /// </summary>
    /// <returns></returns>
    // [Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [AllowAnonymous] // Temporarily allowing anonymous access for testing purposes
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TeamDto>>> GetAllTeams()
    {
        var query = new GetAllTeamsQuery();
        var teams = await mediator.Send(query);
        return Ok(teams);
    }

    /// <summary>
    /// Retrieves a team by its unique identifier.
    /// This endpoint allows you to get the details of a specific team based on its ID.
    /// If the team with the specified ID does not exist, a 404 Not Found response will be returned.
    /// If the team is found, a 200 OK response with the team details will be returned.
    /// This endpoint is useful for retrieving information about a specific team, such as its name,
    /// members, and team manager.
    /// Authorization is not required for this endpoint, but it can be restricted to specific roles
    /// such as "Admin" or "Manager" if needed.
    /// Example usage:
    /// GET /teams/{teamId}
    /// where `{teamId}` is the unique identifier of the team you want to retrieve.
    /// </summary>
    /// <param name="teamId"></param>
    /// <returns></returns>
    // [Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [HttpGet("{teamId:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDto>> GetTeam(Guid teamId)
    {
        var team = await mediator.Send(new GetTeamQuery(teamId));
        return Ok(team);
    }

    /// <summary>
    ///  Retrieve all teams managed by a specific manager.
    ///  This endpoint allows you to get a list of teams based on the manager's ID
    ///  and optionally include the members of those teams.
    ///  If `includeMembers` is set to true, the response will include the members of each team.
    ///  If `includeMembers` is false, the response will only include the team details without member information.
    ///  If no teams are found for the given manager ID,
    ///  a 404 Not Found response will be returned.
    ///  This endpoint is useful for managers to view the teams they oversee and their members.
    ///  It can also be used by administrators to manage teams based on their managers.
    ///  Authorization is not required for this endpoint, but it can be restricted to specific roles
    ///  such as "Admin" or "Manager" if needed.
    ///  Example usage:
    ///  GET /teams/manager?managerId=123e4567-e89b-12d3-a456-426614174000&includeMembers=true
    ///  This will return a list of teams managed by the manager with ID `123e4567-e89b-12d3-a456-426614174000`.
    /// </summary>
    /// <param name="managerId"></param>
    /// <param name="includeMembers"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("manager")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TeamDto>>> GetTeamsByManagerId(
        [FromQuery] Guid managerId,
        [FromQuery] bool includeMembers = false,
        CancellationToken cancellationToken = default
    )
    {
        var teams = await mediator.Send(
            new GetTeamsByManagerQuery(managerId, includeMembers),
            cancellationToken
        );
        return Ok(teams);
    }

    /// <summary>
    /// Retrieves all teams that a specific member is part of.
    /// This endpoint allows you to get a list of teams based on the member's #if true
    /// unique identifier (memberId)
    /// and optionally include the members of those teams.
    /// If `includeMembers` is set to true, the response will include the members of each team.
    /// If `includeMembers` is false, the response will only include the team details without member information.
    /// If no teams are found for the given member ID, a 404 Not Found response will be returned.
    /// This endpoint is useful for members to view the teams they are part of and their members.
    /// It can also be used by administrators to manage teams based on their members.
    /// Authorization is not required for this endpoint, but it can be restricted to specific roles
    /// such as "Admin" or "Manager" if needed.
    /// Example usage:
    /// GET /teams/member?memberId=123e4567-e89b-12d3-a456-426614174000&amp;includeMembers=true
    /// This will return a list of teams that the member with ID `123e4567-e89b-12d3-a456-426614174000` is part of.
    /// </summary>
    /// <param name="memberId"></param>
    /// <param name="includeMembers"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    // [Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [HttpGet("member")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TeamDto>>> GetTeamsByMemberId(
        [FromQuery] Guid memberId,
        [FromQuery] bool includeMembers = false,
        CancellationToken cancellationToken = default
    )
    {
        var teams = await mediator.Send(
            new GetTeamsByMemberQuery(memberId, includeMembers),
            cancellationToken
        );
        return Ok(teams);
    }

    /// <summary>
    /// Adds a new member to the team.
    /// This endpoint allows an administrator or team manager to add a new member to a specific team.
    /// If the member with the specified ID does not exist, a 404 Not Found response will be returned.
    /// If the addition is successful, a 201 Created response with the team details will be returned.
    /// This endpoint is useful for managing team memberships and ensuring that teams are up-to-date with their members.
    /// Authorization is required for this endpoint, typically restricted to users with the "Admin" or "Manager" role.
    /// Example usage:
    /// POST /teams/member?memberId=123e4567-e89b-12d3-a456-426614174000
    /// where `memberId` is the unique identifier of the member to be added to the team.
    /// The request will create a new team member and associate them with the team.
    /// </summary>
    /// <param name="memberId"></param>
    /// <returns></returns>
    //[Authorize(Roles = "Manager(responsable d'équipe)")] tous les deux admin et manager
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous] // Temporarily allowing anonymous access for testing purposes
    [HttpPost("member")]
    public async Task<ActionResult<TeamDto>> AddTeamMember([FromQuery] Guid memberId)
    {
        await employeeService.AddTeamMemberAsync(memberId);
        return NoContent();
    }

    /// <summary>
    /// Creates a new team with the specified details.
    /// This endpoint allows an administrator or team manager to create a new team in the system.
    /// If the creation is successful, a 201 Created response with the created team details will be returned.
    /// If the request data is invalid, a 400 Bad Request response with validation          errors will be returned.
    /// This endpoint is useful for managing teams, allowing users to organize members into specific groups.
    /// Authorization is required for this endpoint, typically restricted to users with the "Admin" or "Manager" role.
    /// Example usage:
    /// POST /teams
    /// {
    ///   "name": "Development Team",
    ///   "memberId": ["123e4567-e89b-12d3-a456-426614174000", "123e4567-e89b-12d3-a456-426614174001"],
    ///   "teamManagerId": "123e4567-e89b-12d3-a456-426614174002"
    /// }
    /// The request body should contain the team name, a list of member IDs, and the team manager ID.
    /// The `teamManagerId` should be a valid user ID of a user who will manage the team.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    //[Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CreateTeamCommand>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamCommand command)
    {
        var validationResult = await createTeamValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errorResponse = ValidationErrorMapper.MapErrors(validationResult.Errors);
            return BadRequest(errorResponse);
        }
        var createdTeam = await mediator.Send(command);
        return CreatedAtAction(nameof(GetTeam), new { teamId = createdTeam.Id }, createdTeam);
    }

    /// <summary>
    /// Updates an existing team by its unique identifier.
    /// This endpoint allows an administrator or team manager to modify the details of a specific team.
    /// If the team with the specified ID does not exist, a 404 Not Found response will be returned.
    /// If the update is successful, a 200 OK response with the updated team details will be returned.
    /// This endpoint is useful for managing team information, such as changing the team name,
    /// updating team members, or changing the team manager.
    /// Authorization is required for this endpoint, typically restricted to users with the "Admin" or "Manager" role.
    /// Example usage:
    /// PUT /teams/{teamId}
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174000",
    ///   "name": "Updated Team Name",
    ///   "memberId": ["123e4567-e89b-12d3-a456-426614174001", "123e4567-e89b-12d3-a456-426614174002"],
    ///   "teamManagerId": "123e4567-e89b-12d3-a456-426614174003"
    /// }
    /// where `teamId` is the unique identifier of the team to be updated.
    /// The request body should contain the updated team details, including the team ID, name,
    /// member IDs, and the team manager ID.
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    //[Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [HttpPut("{teamId}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamRequestDto>> UpdateTeamById(
        Guid teamId,
        [FromBody] UpdateTeamCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (teamId != command.Id)
            return BadRequest("Team ID in the URL does not match the ID in the request body.");
        var validationResult = await updateTeamValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorResponse = ValidationErrorMapper.MapErrors(validationResult.Errors);
            return BadRequest(errorResponse);
        }
        var team = await mediator.Send(command, cancellationToken);
        return Ok(team);
    }

    // ajouter une méthode ici qui gère TeamEvent (hangfire + authorize [il faut une authentification pour ajouter un membre dans une équipe])

    /// <summary>
    /// Deletes a team member by their unique identifier and the name of the team.
    /// This endpoint allows an administrator or team manager to remove a member from a specific team.
    /// If the member with the specified ID does not exist in the team, a 404 Not Found response will be returned.
    /// If the deletion is successful, a 204 No Content response will be returned.
    /// This endpoint is useful for managing team memberships and ensuring that only active members are retained in the team.
    /// Authorization is required for this endpoint, typically restricted to users with the "Admin" or "Manager" role.
    /// Example usage:
    /// DELETE /teams/member
    /// {
    ///   "memberId": "123e4567-e89b-12d3-a456-426614174000",
    ///   "teamName": "Development Team"
    /// }
    /// where `memberId` is the unique identifier of the member to be deleted and `teamName` is the name of the team from which the member will be removed.
    /// </summary>
    /// <param name="deleteTeamMemberDto"></param>
    /// <returns></returns>
    // [Authorize(Roles = "Manager(responsable d'équipe)")]
    [HttpDelete("member")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTeamMemberById(
        [FromBody] DeleteTeamMemberDto deleteTeamMemberDto
    ) // doit etre pareil que AddTeamMember  c'est à dire vient depuis un service externe
    {
        if (deleteTeamMemberDto == null)
            return BadRequest("Request data cannot be null.");
        if (string.IsNullOrWhiteSpace(deleteTeamMemberDto.TeamName))
            return BadRequest("Team name must be provided.");
        // await mediator.Send(
        //     new DeleteTeamMemberCommand(deleteTeamMemberDto.MemberId, deleteTeamMemberDto.TeamName)
        // );
        await employeeService.DeleteTeamMemberAsync(
            deleteTeamMemberDto.MemberId,
            deleteTeamMemberDto.TeamName
        );
        return NoContent();
    }

    /// <summary>
    /// Deletes a team by its unique identifier.
    /// This endpoint allows an administrator to delete a team from the system.
    /// If the team with the specified ID does not exist, a 404 Not Found response will be returned.
    /// If the deletion is successful, a 204 No Content response will be returned.
    /// This endpoint is useful for managing teams and ensuring that only active teams are retained in the system.
    /// Authorization is required for this endpoint, typically restricted to users with the "Admin" role.
    /// Example usage:
    /// DELETE /teams/{teamId}
    /// where `{teamId}` is the unique identifier of the team to be deleted.
    /// </summary>
    /// <param name="teamId"></param>
    /// <returns></returns>
    //[Authorize(Roles = "Admin")]
    [HttpDelete("{teamId:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTeam(Guid teamId)
    {
        await mediator.Send(new DeleteTeamCommand(teamId, null!));
        return NoContent();
    }

    // Il faut une méthode pour changer le manager d'une équipe
}
