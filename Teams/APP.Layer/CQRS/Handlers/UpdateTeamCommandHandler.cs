using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Services;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class UpdateTeamCommandHandler(ITeamRepository teamRepository, IMapper mapper)
    : IRequestHandler<UpdateTeamCommand, TeamRequestDto>
{
    public async Task<TeamRequestDto> Handle(
        UpdateTeamCommand command,
        CancellationToken cancellationToken
    )
    {
        var existingTeam = await teamRepository.GetTeamByIdAsync(command.Id)!;
        if (existingTeam == null)
        {
            throw new HandlerException(
                404,
                $"A team with the Id '{command.Id}' not found.",
                "Not Found",
                "Team ID not found"
            );
        }
        if (
            existingTeam.Name == command.Name
            && existingTeam.MemberId.SequenceEqual(command.MemberId)
        )
        {
            throw new HandlerException(
                400,
                "No changes detected in the team details.",
                "Bad Request",
                "No changes to update"
            );
        }
        existingTeam.Name = command.Name!;
        existingTeam.TeamManagerId = command.TeamManagerId;
        existingTeam.MemberId = command.MemberId;
        await teamRepository.UpdateTeamAsync(existingTeam);
        return mapper.Map<TeamRequestDto>(existingTeam);
    }
}
