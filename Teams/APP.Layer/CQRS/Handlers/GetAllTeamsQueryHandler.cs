using MediatR;
using Teams.CORE.Layer.Interfaces;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Queries;
namespace Teams.APP.Layer.CQRS.Handlers;

public class GetAllTeamsQueryHandler : IRequestHandler<GetAllTeamsQuery, List<TeamDto>>
{
    private readonly ITeamRepository teamRepository;
    public GetAllTeamsQueryHandler(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }

    public async Task<List<TeamDto>> Handle(GetAllTeamsQuery request, CancellationToken cancellationToken)
    {
        
        var teams = await teamRepository.GetAllTeamsAsync();
        var teamDtos = teams.Select(team => new TeamDto
        {
            Name = team.Name,
            TeamManagerId= team.TeamManagerId,
            MemberId = team.MemberId
        }).ToList();
        return teamDtos;
    }
}

