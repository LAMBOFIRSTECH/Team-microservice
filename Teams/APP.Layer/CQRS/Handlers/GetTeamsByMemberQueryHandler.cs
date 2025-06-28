using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamsByMemberQueryHandler
    : IRequestHandler<GetTeamsByMemberQuery, List<TeamRequestDto>>
{
    private readonly ITeamRepository teamRepository;

    public GetTeamsByMemberQueryHandler(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }

    public async Task<List<TeamRequestDto>> Handle(
        GetTeamsByMemberQuery request,
        CancellationToken cancellationToken
    )
    {
        var teams = await teamRepository.GetTeamsByMemberIdAsync(request.MemberId);
        if (teams == null || teams.Count.Equals(0)) //Mettre la logique métier dans l'agrégat racine et pas dans le handler (Domain Driven Design)
            throw new HandlerException(
                404,
                $"Team with Member ID {request.MemberId} not found.",
                "Not Found",
                "Team ressource not found"
            );

        // foreach (var team in teams)
        // {
        //     if (team.MemberIds == null || team.MemberIds.Count == 0)
        //     {
        //         team.MemberIds = new List<Guid>();
        //     }
        // }
        var teamDtos = teams
            .Select(team => new TeamRequestDto(
                team.Id,
                team.TeamManagerId,
                team.Name,
                request.IncludeMembers,
                team.MemberIds.ToList()
            ))
            .ToList();
        return teamDtos;
    }
}
