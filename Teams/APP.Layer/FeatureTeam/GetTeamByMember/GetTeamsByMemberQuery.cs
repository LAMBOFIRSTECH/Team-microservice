using MediatR;
using Teams.APP.Layer.DTOs;

namespace Teams.APP.Layer.FeatureTeam.GetTeamByMember;

public sealed class GetTeamsByMemberQuery : IRequest<List<TeamRequestDto>>
{
    public Guid MemberId { get; init;}
    public bool IncludeMembers { get;   init; }

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
