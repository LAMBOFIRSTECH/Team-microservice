using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Entities.ValueObjects;
using Teams.CORE.Layer.CoreInterfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamQueryHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    IRedisCacheService redisCache,
    ILogger<GetTeamQueryHandler> log
) : IRequestHandler<GetTeamQuery, TeamDetailsDto>
{
    private string verdict = string.Empty;
    public Dictionary<TeamState, string> StateMappings =>
       Enum.GetValues(typeof(TeamState))
           .Cast<TeamState>()
           .ToDictionary(state => state, state => verdict); // dépendance majeure à un objet du domaine [TeamState] Couplage fort
    public async Task<TeamDetailsDto> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetTeamByIdAsync(request.Id, cancellationToken);
        if (team is not null) return BuildDto(team, mapper);
        var archivedTeamDto = await redisCache.GetArchivedTeamFromRedisAsync(request.Id, cancellationToken);
        if (archivedTeamDto is not null)
        {
            Enum.TryParse(archivedTeamDto.State, out TeamState currentStatus);
            if (currentStatus == TeamState.Archived)
            {
                archivedTeamDto.State = $"Team {archivedTeamDto.Name} has been archived for 7 days.";
                return archivedTeamDto;
            }
        }
        throw HandlerException.NotFound(
            title: "Not Found",
            statusCode: 404,
            message: $"Team with ID {request.Id} not found.",
            reason: "Resource not found"
        );
    }

    private TeamDetailsDto BuildDto(Team team, IMapper mapper) // Couplage fort avec le domaine à casser
    {
        var teamDto = mapper.Map<TeamDetailsDto>(team);
        var projectAssociation = team.Project;
        if (projectAssociation == null || projectAssociation.Details.Count == 0)
        {
            teamDto.HasAnyProject = false;
            teamDto.ProjectNames = null;
        }
        else
        {
            teamDto.TeamExpirationDate = projectAssociation
                .GetprojectMaxEndDate()
                .ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

            teamDto.HasAnyProject = true;
            teamDto.TeamManagerId = projectAssociation.TeamManagerId;
            teamDto.Name = projectAssociation.TeamName;
            teamDto.ProjectNames = projectAssociation.Details.Select(d => d.ProjectName).ToList();
        }

        teamDto.State = GetMaturity(team);
        return teamDto;
    }
    private string GetMaturity(Team team) // Couplage fort avec le domaine métier à casser
    {
        if (!team.IsMature())
        {
            verdict = "not yet mature";
            _ = StateMappings[team.State];
            LogHelper.Info($"✅ Team is {team.State} however {StateMappings[team.State]}.", log);
            return $"✅ Team is {team.State} with {team.ProjectState} project however {StateMappings[team.State]}.";
        }
        verdict = "Mature";
        _ = StateMappings[team.State];
        LogHelper.Info($"✅ Team is {team.State} and {StateMappings[team.State]}.", log);
        return $"✅ Team is {team.State} with {team.ProjectState} project and {StateMappings[team.State]}.";

    }
}
