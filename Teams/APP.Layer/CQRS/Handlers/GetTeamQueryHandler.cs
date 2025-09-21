using AutoMapper;
using Humanizer;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamQueryHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    IProjectService projectService,
    ILogger<GetTeamQueryHandler> log
) : IRequestHandler<GetTeamQuery, TeamDetailsDto>
{
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
        team.Maturity();
        LogHelper.Info($"✅ Team state is {team.State} and {team.StateMappings[team.State]}.", log);
        var teamDto = mapper.Map<TeamDetailsDto>(team);
        // var check= On va traiter une fois l'event du domaine apporter
        var projectAssociations = await projectService.GetProjectAssociationDataAsync(null, teamDto.Name);
        teamDto.ProjectNames = projectAssociations.Details.Select(d => d.ProjectName).ToList();
        return teamDto;
    }
}
