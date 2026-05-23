using System.Collections.Immutable;
namespace Teams.APP.Layer.FeatureTeam.UpdateTeam;
public record UpdateTeamRequest(Guid Id, string Name, Guid TeamManagerId, ImmutableArray<Guid> MembersIds);