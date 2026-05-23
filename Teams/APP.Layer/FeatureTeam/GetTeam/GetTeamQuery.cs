using MediatR;
using Teams.APP.Layer.DTOs.Output;

namespace Teams.APP.Layer.FeatureTeam.GetTeam;

public sealed class GetTeamQuery : IRequest<TeamDtoModels.TeamDetailsDto>
{
    public Guid Id { get; init;}
    public GetTeamQuery(Guid identifier) => Id = identifier;
}
