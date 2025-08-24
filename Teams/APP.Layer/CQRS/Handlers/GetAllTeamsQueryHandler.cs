using MediatR;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Queries;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetAllTeamsQueryHandler : IRequestHandler<GetAllTeamsQuery, List<TeamDto>>
{
    private readonly ITeamRepository teamRepository;

    public GetAllTeamsQueryHandler(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }

    public async Task<List<TeamDto>> Handle(
        GetAllTeamsQuery request,
        CancellationToken cancellationToken
    )
    {
        var teams = await teamRepository.GetAllTeamsAsync();
        if (request.OnlyMature)
        {
            var now = DateTime.UtcNow;
            teams = teams.Where(t => (now - t.TeamCreationDate).TotalSeconds >= 30).ToList();
        }

        var teamDtos = teams
            .Select(team => new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                TeamManagerId = team.TeamManagerId,
                MembersId = team.MembersIds.ToList(),
            })
            .ToList();
        return teamDtos;
    }
}
