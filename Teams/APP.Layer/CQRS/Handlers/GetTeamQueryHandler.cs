using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamQueryHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    ILogger<GetTeamQueryHandler> log
) : IRequestHandler<GetTeamQuery, TeamDetailsDto>
{
    private string verdict = string.Empty;
    public Dictionary<TeamState, string> StateMappings =>
       Enum.GetValues(typeof(TeamState))
           .Cast<TeamState>()
           .ToDictionary(state => state, state => verdict);
    public async Task<TeamDetailsDto> Handle(
        GetTeamQuery request,
        CancellationToken cancellationToken
    )
    {
        var team =
            await teamRepository.GetTeamByIdAsync(request.Id, cancellationToken)
            ?? throw new HandlerException(
                404,
                $"Team with ID {request.Id} not found.",
                "Not Found",
                "Team ressource not found"
            );
        if (team.State == TeamState.Archivee)
            throw new HandlerException(
                410,
                $"Team with ID {request.Id} is expired.",
                "Gone",
                "Team ressource is expired"
            );
        var teamDto = mapper.Map<TeamDetailsDto>(team);
        var projectAssociation = team.Project;
        if (!team.HasAnyDependencies() || projectAssociation == null || projectAssociation.Details.Count == 0)
        {
            teamDto.ActiveProject = false;
            teamDto.ProjectNames = null;
            teamDto.State = Maturity(team);
            return teamDto;
        }

        teamDto.ActiveProject = true;
        teamDto.TeamManagerId = projectAssociation.TeamManagerId;
        teamDto.Name = projectAssociation.TeamName;
        teamDto.ProjectNames = projectAssociation.Details.Select(d => d.ProjectName).ToList();
        teamDto.State = Maturity(team);
        return teamDto;
    }
    public string Maturity(Team team)
    {
        if (!team.IsMature())
        {
            verdict = "not yet mature";
            _ = StateMappings[team.State];
            LogHelper.Info($"✅ Team is {team.State} however {StateMappings[team.State]}.", log);
            return $"✅ Team is {team.State} however {StateMappings[team.State]}.";
        }
        verdict = "Mature";
        _ = StateMappings[team.State];
        LogHelper.Info($"✅ Team is {team.State} and {StateMappings[team.State]}.", log);
        return $"✅ Team is {team.State} and {StateMappings[team.State]}.";

    }
}
