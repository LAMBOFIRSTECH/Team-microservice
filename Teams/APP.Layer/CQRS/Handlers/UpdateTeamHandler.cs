using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class UpdateTeamHandler(ITeamRepository teamRepository, IMapper mapper)
    : IRequestHandler<UpdateTeamCommand, TeamRequestDto>
{
    public async Task<TeamRequestDto> Handle(
        UpdateTeamCommand command,
        CancellationToken cancellationToken
    )
    {
        var existingTeam = await teamRepository.GetTeamByIdAsync(command.Id, cancellationToken);
        if (existingTeam == null)
        {
            throw new HandlerException(
                404,
                $"A team with the Id '{command.Id}' not found.",
                "Not Found",
                "Team ID not found"
            );
        }
        try
        {
            existingTeam.UpdateTeam(command.Name!, command.TeamManagerId, command.MemberId);
        }
        catch (DomainException ex)
        {
            throw HandlerException.BadRequest(ex.Message, "Validation Error");
        }
        await teamRepository.UpdateTeamAsync(existingTeam, cancellationToken);
        return mapper.Map<TeamRequestDto>(existingTeam);
    }
}
