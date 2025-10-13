using MediatR;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Queries;
using Teams.CORE.Layer.Entities.TeamAggregate;
using NodaTime;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetAllTeamsQueryHandler(ITeamRepository teamRepository)
    : IRequestHandler<GetAllTeamsQuery, List<TeamDto>>
{
    public async Task<List<TeamDto>> Handle(
        GetAllTeamsQuery request,
        CancellationToken cancellationToken
    )
    {
        var teams = await teamRepository.GetAllTeamsAsync(cancellationToken);
        if (request.OnlyMature)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            teams = teams.Where(t => (now - t.TeamCreationDate.ToInstant()).TotalSeconds >= 30).ToList();
        }

        var teamDtos = teams
            .Select(team => new TeamDto
            {
                Id = team.Id,
                Name = team.Name.Value,
                TeamManagerId = team.TeamManagerId.Value,
                MembersIds = team.MembersIds.Select(m=>m.Value).ToHashSet(),
            })
            .ToList();
        return teamDtos;
    }
}
