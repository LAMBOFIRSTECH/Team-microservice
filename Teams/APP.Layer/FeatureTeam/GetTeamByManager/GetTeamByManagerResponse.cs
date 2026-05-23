using FluentValidation;

namespace Teams.APP.Layer.FeatureTeam.GetTeamByManager;
// c'est la response du get team by manager, elle contient une liste de team details dto
// a revoir
public class GetTeamByManagerResponse
{    public IEnumerable<TeamDtoModels.TeamDetailsDto> Teams { get; init; }
    public GetTeamByManagerResponse(IEnumerable<TeamDtoModels.TeamDetailsDto> teams)
    {        Teams = teams;
    }
}