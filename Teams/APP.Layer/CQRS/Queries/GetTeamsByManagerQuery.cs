using MediatR;
using Teams.API.Layer.DTOs;
namespace Teams.APP.Layer.CQRS.Queries;
public class GetTeamsByManagerQuery : IRequest<List<TeamRequestDto>>
{
    public Guid TeamManagerId { get; }
    public bool IncludeMembers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTeamsByManagerQuery"/> class with the specified team identifier.
    /// </summary>
    /// <param name="TeamManagerId">The unique identifier of the team manager to retrieve.</param>
    /// <param name="includeMembers">Whether to include team members in the result.</param>
    public GetTeamsByManagerQuery(Guid TeamManagerId, bool includeMembers = false)
    {
        this.TeamManagerId = TeamManagerId;
        IncludeMembers = includeMembers;
    }
}
