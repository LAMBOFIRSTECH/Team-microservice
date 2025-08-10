using AutoMapper;
using MediatR;
using Serilog;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class UpdateTeamManagerHandler(
    ITeamRepository teamRepository,
    ILogger<UpdateTeamManagerHandler> logger
) : IRequestHandler<UpdateTeamManagerCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateTeamManagerCommand command,
        CancellationToken cancellationToken
    )
    {
        var existingTeam = await teamRepository.GetTeamByNameAndTeamManagerIdAsync(
            command.Name!,
            command.OldTeamManagerId
        )!;
        if (existingTeam == null)
        {
            LogHelper.Error(
                $"Team with Name: {command.Name} and Old Team Manager Id: {command.OldTeamManagerId} not found.",
                logger
            );
            throw new HandlerException(
                404,
                $"A team with Name : '{command.Name}' and Manager Id : '{command.OldTeamManagerId}' not found.",
                "Not Found",
                "Team ID not found"
            );
        }
        try
        {
            existingTeam.ChangeTeamManager(command.NewTeamManagerId);
            LogHelper.Info(
                $"Team manager changed successfully for team -- {command.Name} --",
                logger
            );
        }
        catch (DomainException ex)
        {
            throw HandlerException.BadRequest(ex.Message, "Validation Error");
        }
        await teamRepository.UpdateTeamAsync(existingTeam);
        return Unit.Value;
    }
}
