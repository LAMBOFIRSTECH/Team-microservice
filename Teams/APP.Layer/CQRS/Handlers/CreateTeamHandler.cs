using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class CreateTeamHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    ILogger<CreateTeamHandler> log
) : IRequestHandler<CreateTeamCommand, TeamDto>
{
    public async Task<TeamDto> Handle(
        CreateTeamCommand command,
        CancellationToken cancellationToken
    )
    {
        var existingTeams = await teamRepository.GetAllTeamsAsync();
        Team team;
        try
        {
            team = Team.Create(
                command.Name!,
                command.TeamManagerId,
                command.MembersId,
                existingTeams,
                false,
                null,
                null
            );
        }
        catch (DomainException ex)
        {
            LogHelper.BusinessRuleFailure(log, "Team creation ", $"{ex.Message}", null);
            throw new HandlerException(400, ex.Message, "Bad Request", "Domain Validation Error");
        }
        await teamRepository.CreateTeamAsync(team);
        return mapper.Map<TeamDto>(team);
    }
}
