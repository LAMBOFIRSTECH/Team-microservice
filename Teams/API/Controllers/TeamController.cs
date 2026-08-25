using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Teams.APP.Features.CreateTeam;
namespace Teams.API.Controllers;

[ApiController]
[Route("teams")]
public sealed class TeamController(ISender _mediator) : ControllerBase
{
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
    [ProducesResponseType<CreateTeamModels.CreateTeamResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTeam), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTeam(Guid id, CancellationToken cancellationToken)
    {
        // Ta route de récupération pour le CreatedAtAction
        return Ok();
    }
}