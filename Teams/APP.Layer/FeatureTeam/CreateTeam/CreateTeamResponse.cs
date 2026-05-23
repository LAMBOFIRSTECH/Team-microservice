using System.Collections.Immutable;
namespace Teams.APP.Layer.FeatureTeam.CreateTeam;
public record CreateTeamResponse(Guid Id, string Name, Guid TeamManagerId, ImmutableArray<Guid> MembersIds);