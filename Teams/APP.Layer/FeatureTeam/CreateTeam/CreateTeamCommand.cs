using System.Collections.Immutable;
using MediatR;
namespace Teams.APP.Layer.FeatureTeam.CreateTeam;
public record CreateTeamCommand(string Name, Guid TeamManagerId, ImmutableArray<Guid> MembersIds) : IRequest<CreateTeamResponse>;