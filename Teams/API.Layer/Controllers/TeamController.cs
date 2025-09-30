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
    /// </summary>
    /// <remarks>
    /// This endpoint allows you to get a list of all teams, including their details such as team name, members, and team manager.
    /// 
    /// **Authorization:** Not required by default, but can be restricted to specific roles such as "Admin" or "Manager".
    /// 
    /// **Query Parameters:**
    /// - `onlyMature` (bool, optional): If true, only mature teams will be returned.
    ///
    /// **Example usage:**
    /// ```http
    /// GET /teams?onlyMature=true
    /// ```
    ///
    /// **Response:**
    /// - 200 OK: Returns a list of `TeamDto` objects. If no teams are found, an empty list is returned.
    /// </remarks>
    /// <param name="onlyMature">Filter to return only mature teams (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of teams as `TeamDto`</returns>
    // [Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [AllowAnonymous]
    [HttpGet]
    [Consumes("application/json")]
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
    /// </summary>
    /// <remarks>
    /// This endpoint allows you to get the details of a specific team, including its name, members, and team manager.
    ///
    /// **Authorization:** Not required by default, but can be restricted to specific roles such as "Admin" or "Manager".
    ///
    /// **Route Parameters:**
    /// - `teamId` (GUID): The unique identifier of the team to retrieve.
    ///
    /// **Example usage:**
    /// ```http
    /// GET /teams/{teamId}
    /// ```
    ///
    /// **Responses:**
    /// - 200 OK: Returns the team details as `TeamDetailsDto`.
    /// - 404 Not Found: The team with the specified ID does not exist.
    /// </remarks>
    /// <param name="teamId">The unique identifier of the team to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The details of the requested team as `TeamDetailsDto`</returns>
    // [Authorize(Policy = "AdminPolicy,ManagerPolicy(responsable d'équipe)")]
    [AllowAnonymous]
    [HttpGet("{teamId:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TeamDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDetailsDto>> GetTeam(
        Guid teamId,
        CancellationToken cancellationToken = default
    )
    {
        var teamDto = await _mediator.Send(new GetTeamQuery(teamId), cancellationToken);
        return Ok(teamDto);
    }

    /// <summary>
    /// Retrieves statistics for a specific team.
    /// </summary>
    /// <remarks>
    /// This endpoint returns various statistics related to a team, such as the number of members, active projects, or other metrics.
    ///
    /// **Query Parameters:**
    /// - `teamId` (GUID, required): The unique identifier of the team for which stats are requested.
    ///
    /// **Example usage:**
    /// ```http
    /// GET /teams/stats?teamId=123e4567-e89b-12d3-a456-426614174000
    /// ```
    ///
    /// **Responses:**
    /// - 200 OK: Returns the team statistics as `TeamStatsDto`.
    /// - 404 Not Found: The team with the specified ID does not exist.
    /// </remarks>
    /// <param name="teamId">The unique identifier of the team</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The statistics of the requested team as `TeamStatsDto`</returns>
    [AllowAnonymous]
    [HttpGet("stats")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TeamStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDetailsDto>> GetTeamStats(
        [FromQuery] Guid teamId,
        CancellationToken cancellationToken = default
    )
    {
        var teamDto = await _mediator.Send(new GetTeamStatsQuery(teamId), cancellationToken);
        return Ok(teamDto);
    }


    /// <summary>
    /// Retrieves all teams managed by a specific manager.
    /// </summary>
    /// <remarks>
    /// This endpoint returns a list of teams based on the manager's ID. Optionally, it can include the members of those teams.
    ///
    /// **Query Parameters:**
    /// - `managerId` (GUID, required): The unique identifier of the manager.
    /// - `includeMembers` (bool, optional, default = false): If true, include team members in the response.
    ///
    /// **Responses:**
    /// - 200 OK: Returns a list of teams as `TeamDto`.
    /// - 404 Not Found: No teams were found for the given manager ID.
    ///
    /// **Example usage:**
    /// ```http
    /// GET /teams/manager?managerId=123e4567-e89b-12d3-a456-426614174000%includeMembers=true
    /// ```
    /// This will return all teams managed by the specified manager.
    /// </remarks>
    /// <param name="managerId">The unique identifier of the manager</param>
    /// <param name="includeMembers">Whether to include team members in the response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of teams managed by the specified manager</returns>
    [AllowAnonymous]
    [HttpGet("manager")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TeamDto>>> GetTeamsByManagerId(
        [FromQuery] Guid managerId,
        [FromQuery] bool includeMembers = false,
        CancellationToken cancellationToken = default
    )
    {
        var teamsDto = await _mediator.Send(
            new GetTeamsByManagerQuery(managerId, includeMembers),
            cancellationToken
        );
        return Ok(teamsDto);
    }

    /// <summary>
    /// Changes the team manager for a specific team.
    /// </summary>
    /// <remarks>
    /// This endpoint allows updating the team manager for a team identified by its name and the current manager's ID.
    ///
    /// **Request Body:**
    /// ```json
    /// {
    ///    "Name": "Pentester",
    ///    "OldTeamManagerId": "b14db1e2-026e-4ac9-9739-378720de6f5b",
    ///    "NewTeamManagerId": "9a57d8f7-56f4-47d9-a429-5f4f34e9bc83",
    ///    "ContratType": "Stagiaire"
    /// }
    /// ```
    /// 
    /// **Responses:**
    /// - 204 No Content: The team manager has been successfully changed.
    /// - 400 Bad Request: Validation errors in the request body.
    /// - 404 Not Found: The team with the specified name and current manager ID does not exist.
    /// - 500 Internal Server Error: Unexpected server error.
    ///
    /// **Authorization:** Required, typically restricted to "Admin" or "Manager".
    /// </remarks>
    /// <param name="command">The update command containing team name, old manager ID, new manager ID, and contract type.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful; otherwise, appropriate error response.</returns>
    // [Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [AllowAnonymous]
    [Consumes("application/json")]
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
        if (!validationResult.IsValid) return BadRequest();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Retrieves all teams that a specific member is part of.
    /// </summary>
    /// <remarks>
    /// This endpoint returns a list of teams based on the member's unique identifier (`memberId`).
    /// 
    /// **Query Parameters:**
    /// - `memberId` (Guid): Unique identifier of the member.
    /// - `includeMembers` (bool): If true, includes members of each team; otherwise, only team details are returned.
    ///
    /// **Responses:**
    /// - 200 OK: Returns a list of teams the member is part of.
    /// - 404 Not Found: No teams found for the given member ID.
    ///
    /// **Example usage:**
    /// GET /teams/member?memberId=123e4567-e89b-12d3-a456-426614174000%includeMembers=true
    /// 
    /// This returns all teams that the member with ID `123e4567-e89b-12d3-a456-426614174000` belongs to.
    ///
    /// **Authorization:** Optional; can be restricted to "Admin" or "Manager".
    /// </remarks>
    /// <param name="memberId">Unique identifier of the member</param>
    /// <param name="includeMembers">Whether to include team members in the response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of teams the member is part of</returns>
    /// [Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [AllowAnonymous]
    [HttpGet("member")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(List<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TeamDto>>> GetTeamsByMemberId(
        [FromQuery] Guid memberId,
        [FromQuery] bool includeMembers = false,
        CancellationToken cancellationToken = default
    )
    {
        var teamsDto = await _mediator.Send(
            new GetTeamsByMemberQuery(memberId, includeMembers),
            cancellationToken
        );
        return Ok(teamsDto);
    }

    /// <summary>
    /// Adds a new member to a team.
    /// </summary>
    /// <remarks>
    /// This endpoint allows an administrator or team manager to add a new member to a specific team.
    ///
    /// **Query Parameters:**
    /// - `memberId` (Guid): Unique identifier of the member to be added.
    ///
    /// **Responses:**
    /// - 204 No Content: The member was successfully added to the team.
    /// - 400 Bad Request: The `memberId` is empty or invalid.
    /// - 404 Not Found: The member with the specified ID does not exist or cannot be found in any team.
    /// - 500 Internal Server Error: Unexpected server error.
    ///
    /// **Example usage:**
    /// PATCH /teams/member?memberId=123e4567-e89b-12d3-a456-426614174000
    ///
    /// **Authorization:** Required for "Admin" or "Manager" roles.
    /// </remarks>
    /// <param name="memberId">Unique identifier of the member to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    //[Authorize(Roles = "Manager(responsable d'équipe)")] tous les deux admin et manager
    [Consumes("application/json")]
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
    /// Creates a new team.
    /// </summary>
    /// <remarks>
    /// This endpoint allows an administrator or team manager to create a new team in the system.
    ///
    /// **Request Body:**
    /// ```json
    /// {
    ///   "Name": "Development Team",
    ///   "TeamManagerId": "123e4567-e89b-12d3-a456-426614174002",
    ///   "MembersIds": [
    ///     "123e4567-e89b-12d3-a456-426614174000",
    ///     "123e4567-e89b-12d3-a456-426614174001",
    ///     "123e4567-e89b-12d3-a456-426614174002"
    ///   ]
    /// }
    /// ```
    ///
    /// **Responses:**
    /// - 201 Created: Returns the created team details.
    /// - 400 Bad Request: The request data is invalid.
    ///
    /// **Authorization:** Required for "Admin" or "Manager" roles.
    ///
    /// **Example usage:**
    /// POST /teams
    /// </remarks>
    /// <param name="command">The details of the team to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created team</returns>
    //[Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [AllowAnonymous]
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<CreateTeamCommand>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TeamDto>> CreateTeam(
        [FromBody] CreateTeamCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await _createTeamValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) return BadRequest();
        var createdTeam = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTeam), new { teamId = createdTeam.Id }, createdTeam);
    }

    /// <summary>
    /// Updates an existing team by its unique identifier.
    /// </summary>
    /// <remarks>
    /// This endpoint allows an administrator or team manager to modify the details of a specific team.
    ///
    /// **Request Body:**
    /// ```json
    /// {
    ///   "id": "123e4567-e89b-12d3-a456-426614174000",
    ///   "name": "Updated Team Name",
    ///   "memberId": [
    ///     "123e4567-e89b-12d3-a456-426614174001",
    ///     "123e4567-e89b-12d3-a456-426614174002"
    ///   ],
    ///   "teamManagerId": "123e4567-e89b-12d3-a456-426614174003"
    /// }
    /// ```
    ///
    /// **Responses:**
    /// - 200 OK: Returns the updated team details.
    /// - 400 Bad Request: Validation failed or teamId mismatch between URL and body.
    /// - 404 Not Found: Team with the specified ID does not exist.
    ///
    /// **Authorization:** Required for "Admin" or "Manager" roles.
    ///
    /// **Example usage:**
    /// PUT /teams/{teamId}
    /// </remarks>
    /// <param name="teamId">The unique identifier of the team to update</param>
    /// <param name="command">Updated team details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team</returns>
    //[Authorize(Roles = "Admin,Manager(responsable d'équipe)")]
    [AllowAnonymous]
    [HttpPut("{teamId}")]
    [Consumes("application/json")]
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
        if (!validationResult.IsValid) return BadRequest();
        var teamDto = await _mediator.Send(command, cancellationToken);
        return Ok(teamDto);
    }

    /// <summary>
    /// Deletes a team member from a specific team.
    /// </summary>
    /// <remarks>
    /// This endpoint allows an administrator or team manager to remove a member from a specific team.
    ///
    /// **Request Body:**
    /// ```json
    /// {
    ///   "memberId": "123e4567-e89b-12d3-a456-426614174000",
    ///   "teamName": "Development Team"
    /// }
    /// ```
    ///
    /// **Responses:**
    /// - 204 No Content: Member was successfully removed.
    /// - 400 Bad Request: Request data is null or team name is missing.
    /// - 404 Not Found: Member not found in the specified team.
    ///
    /// **Authorization:** Required for "Admin" or "Manager" roles.
    ///
    /// **Example usage:**
    /// DELETE /teams/member
    /// </remarks>
    /// <param name="deleteTeamMemberDto">DTO containing memberId and teamName</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    // [Authorize(Roles = "Manager(responsable d'équipe)")]
    [AllowAnonymous]
    [HttpDelete("member")]
    [Consumes("application/json")]
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
    /// </summary>
    /// <remarks>
    /// This endpoint allows an administrator to remove a team from the system.
    ///
    /// **Responses:**
    /// - 204 No Content: Team successfully deleted.
    /// - 404 Not Found: Team with the specified ID does not exist.
    ///
    /// **Authorization:** Required for "Admin" role.
    ///
    /// **Example usage:**
    /// DELETE /teams/{teamId}
    /// where `{teamId}` is the unique identifier of the team to be deleted.
    /// </remarks>
    /// <param name="teamId">Unique identifier of the team to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    //[Authorize(Policy = "AdminPolicy")]
    [AllowAnonymous]
    [HttpDelete("{teamId:guid}")]
    [Consumes("application/json")]
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
