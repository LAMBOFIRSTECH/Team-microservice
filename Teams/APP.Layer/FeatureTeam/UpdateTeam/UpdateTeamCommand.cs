using System.Collections.Immutable;
using MediatR;
namespace Teams.APP.Layer.FeatureTeam.UpdateTeam;
public  record UpdateTeamCommand( Guid Id,string Name, Guid TeamManagerId, ImmutableArray<Guid> MembersIds) : IRequest;