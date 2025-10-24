using MediatR;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Queries;
using NodaTime;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetAllTeamsQueryHandler(IUnitOfWork _unitOfWork)
    : IRequestHandler<GetAllTeamsQuery, List<TeamDto>>
{
    public async Task<List<TeamDto>> Handle(
        GetAllTeamsQuery request,
        CancellationToken cancellationToken
    ) // revoir l'orchestration asynchrone
    {
        var teams = _unitOfWork.TeamRepository.GetAll(cancellationToken);
        var now = SystemClock.Instance.GetCurrentInstant();
        if (request.OnlyMature) teams = teams.Where(t => (now - t.TeamCreationDate.ToInstant()).TotalSeconds >= 30).AsQueryable();


        var teamDtos = teams
            .Select(team => new TeamDto
            {
                Id = team.Id,
                Name = team.Name.Value,
                TeamManagerId = team.TeamManagerId.Value,
                MembersIds = team.MembersIds.Select(m => m.Value).ToHashSet(),
            })
            .ToList();
        return teamDtos;
    }
}
