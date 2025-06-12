using MediatR;
using Teams.API.Layer.DTOs;
namespace Teams.APP.Layer.CQRS.Queries;
public class GetTeamsByMemberQuery : IRequest<List<TeamRequestDto>>
{
    public Guid MemberId { get; }
    public bool IncludeMembers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTeamsByMemberQuery"/> class with the specified team identifier.
    /// </summary>
    /// <param name="MemberId">The unique identifier of the team manager to retrieve.</param>
    /// <param name="includeMembers">Whether to include team members in the result.</param>
    public GetTeamsByMemberQuery(Guid MemberId, bool includeMembers = false)
    {
        this.MemberId = MemberId;
        IncludeMembers = includeMembers;
    }
}
