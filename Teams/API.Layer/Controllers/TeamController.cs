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
    IMediator _mediator,
    IValidator<CreateTeamCommand> _createTeamValidator,
    IValidator<UpdateTeamCommand> _updateTeamValidator,
    IValidator<UpdateTeamManagerCommand> _updateTeamManagerValidator,
    IEmployeeService _employeeService
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
    [AllowAnonymous]
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TeamDto>>> GetAllTeams(
        [FromQuery] bool onlyMature = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = new GetAllTeamsQuery { OnlyMature = onlyMature };
        var teams = await _mediator.Send(query, cancellationToken);
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    // [Authorize(Policy = "AdminPolicy,ManagerPolicy(responsable d'équipe)")]
    [AllowAnonymous]
    [HttpGet("{teamId:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TeamDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDetailsDto>> GetTeam(
        Guid teamId,
        CancellationToken cancellationToken = default
    )
    {
        var team = await _mediator.Send(new GetTeamQuery(teamId), cancellationToken);
        return Ok(team);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("stats")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TeamStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDetailsDto>> GetTeamStats(
        [FromQuery] Guid teamId,
        CancellationToken cancellationToken = default
    )
    {
        var team = await _mediator.Send(new GetTeamStatsQuery(teamId), cancellationToken);
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
    ///  GET /teams/manager?managerId=123e4567-e89b-12d3-a456-426614174000&amp;includeMembers=true
    ///  This will return a list of teams managed by the manager with ID `123e4567-e89b-12d3-a456-426614174000`.
    /// </summary>
    /// <param name="managerId"></param>
    /// <param name="includeMembers"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
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
        var teams = await _mediator.Send(
            new GetTeamsByManagerQuery(managerId, includeMembers),
            cancellationToken
        );
        return Ok(teams);
    }

    /// <summary>
    /// Changes the team manager for a specific team.
    /// This endpoint allows you to change the team manager for a specific team identified by its name
    /// and the ID of the current team manager.
    /// If the team with the specified name and current manager ID does not exist, a 404 Not Found response will be returned.
    /// If the change is successful, a 204 No Content response will be returned.
    /// This endpoint is useful for updating team management responsibilities, such as when a team manager leaves or is replaced.
    /// Authorization is required for this endpoint, typically restricted to users with the "Admin" or "Manager" role.
    /// Example usage:
    /// PATCH /teams/manager
    /// {
    ///    "Name": "Pentester",
    ///    "OldTeamManagerId": "b14db1e2-026e-4ac9-9739-378720de6f5b",
    ///    "NewTeamManagerId": "9a57d8f7-56f4-47d9-a429-5f4f34e9bc83"
    ///     "ContratType": "Stagiaire"
    /// }
    /// The request body should contain the team name, the ID of the current team manager,
    /// and the ID of the new team manager.
    /// The `newTeamManagerId` should be a valid user ID of a user who will become the new team manager.
    /// If the request is successful, a 204 No Content response will be returned,
    /// indicating that the team manager has been successfully changed.
    /// If the request fails due to validation errors, a 400 Bad Request response will be returned
    /// with details about the validation errors.
    /// If the team is not found, a 404 Not Found response will be returned.
    /// If an unexpected error occurs, a 500 Internal Server Error response will be returned.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    // [Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [AllowAnonymous]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPatch("manager")]
    public async Task<IActionResult> ChangeTeamManager(
        [FromBody] UpdateTeamManagerCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _updateTeamManagerValidator.ValidateAsync(
            command,
            cancellationToken
        );
        if (!validationResult.IsValid)
        {
            var errorResponse = ValidationErrorMapper.MapErrors(validationResult.Errors);
            return BadRequest(errorResponse);
        }
        await _mediator.Send(command, cancellationToken);
        return NoContent();
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
    [AllowAnonymous]
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
        var teams = await _mediator.Send(
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    //[Authorize(Roles = "Manager(responsable d'équipe)")] tous les deux admin et manager
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [AllowAnonymous]
    [HttpPatch("member")]
    public async Task<IActionResult> AddTeamMember(
        [FromQuery] Guid memberId,
        CancellationToken cancellationToken = default
    )
    {
        if (memberId == Guid.Empty)
            return BadRequest("Member ID cannot be empty.");

        var result = await _employeeService.InsertNewTeamMemberIntoDbAsync(
            memberId,
            cancellationToken
        );
        if (!result)
        {
            return NotFound($"Member with ID {memberId} not found in any cache team.");
        }
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
    ///   "Name": "Development Team",
    ///   "MembersId": ["123e4567-e89b-12d3-a456-426614174000", "123e4567-e89b-12d3-a456-426614174001"],
    ///   "TeamManagerId": "123e4567-e89b-12d3-a456-426614174002"
    /// }
    /// The request body should contain the team name, a list of member IDs, and the team manager ID.
    /// The `teamManagerId` should be a valid user ID of a user who will manage the team.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    //[Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [AllowAnonymous]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<CreateTeamCommand>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamDto>> CreateTeam(
        [FromBody] CreateTeamCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _createTeamValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorResponse = ValidationErrorMapper.MapErrors(validationResult.Errors);
            return BadRequest(errorResponse);
        }
        var createdTeam = await _mediator.Send(command, cancellationToken);
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
    [AllowAnonymous]
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
        var validationResult = await _updateTeamValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errorResponse = ValidationErrorMapper.MapErrors(validationResult.Errors);
            return BadRequest(errorResponse);
        }
        var team = await _mediator.Send(command, cancellationToken);
        return Ok(team);
    }

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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    // [Authorize(Roles = "Manager(responsable d'équipe)")]
    [AllowAnonymous]
    [HttpDelete("member")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeamMemberById(
        [FromBody] DeleteTeamMemberDto deleteTeamMemberDto,
        CancellationToken cancellationToken = default
    )
    {
        if (deleteTeamMemberDto == null)
            return BadRequest("Request data cannot be null.");
        if (string.IsNullOrWhiteSpace(deleteTeamMemberDto.TeamName))
            return BadRequest("Team name must be provided.");
        await _employeeService.DeleteTeamMemberAsync(
            deleteTeamMemberDto.MemberId,
            deleteTeamMemberDto.TeamName,
            cancellationToken
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
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    //[Authorize(Policy = "AdminPolicy")]
    [AllowAnonymous]
    [HttpDelete("{teamId:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeam(
        Guid teamId,
        CancellationToken cancellationToken = default
    )
    {
        await _mediator.Send(new DeleteTeamCommand(teamId, null!), cancellationToken);
        return NoContent();
    }
}
