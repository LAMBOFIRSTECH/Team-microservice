using System.Collections.Immutable;
using MediatR;

namespace Teams.APP.Features.CreateTeam;
/// <summary>
/// Represents a command to create a new team.
/// </summary>
/// <param name="Name">The name of the team.</param>
/// <param name="TeamManagerId">The ID of the team manager.</param>
/// <param name="MembersIds">The IDs of the team members.</param>
public record CreateTeamCommand(string Name, Guid TeamManagerId, ImmutableArray<Guid> MembersIds) : IRequest<CreateTeamModels.CreateTeamResponse>;