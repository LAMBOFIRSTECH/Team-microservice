using System.Collections.Immutable;
namespace Teams.APP.Features.CreateTeam;

public class CreateTeamModels
{
    public sealed record CreateTeamRequest(Guid Id, string Name, Guid TeamManagerId, ImmutableArray<Guid> MembersIds);
    public sealed record CreateTeamResponse(Guid Id, string Name, Guid TeamManagerId, ImmutableArray<Guid> MembersIds);
}