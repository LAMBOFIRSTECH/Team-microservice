using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Services;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class UpdateTeamCommandHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    EmployeeService employeService
) : IRequestHandler<UpdateTeamCommand, TeamRequestDto>
{
    private async Task<bool> CanMemberJoinNewTeam(Guid memberId)
    {
        var teams = await teamRepository.GetTeamsByMemberIdAsync(memberId);

        if (teams == null || !teams.Any())
            return true; // Le membre n'était dans aucune équipe
        var lastDeparture = await employeService.GetLastTeamLeaveDateAsync(memberId);
        var daysSinceDeparture = DateTime.UtcNow - lastDeparture;
        if (daysSinceDeparture.HasValue && daysSinceDeparture.Value.TotalDays < 7)
            return false; // Moins de 7 jours : refus
        return true;
    }

    public async Task<TeamRequestDto> Handle(
        UpdateTeamCommand command,
        CancellationToken cancellationToken
    )
    {
        var existingTeam = await teamRepository.GetTeamByIdAsync(command.Id)!;
        var uniqueMemberIds = command.MemberId.Distinct().ToList();
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
        foreach (var memberId in uniqueMemberIds)
        {
            if (!await CanMemberJoinNewTeam(memberId))
            {
                throw new HandlerException(
                    400,
                    $"member {memberId} must wait 07 days before been added to a new team.",
                    "Business Rule Violation",
                    "Member Cooldown Period"
                );
            }
        }
        existingTeam.Name = command.Name!;
        existingTeam.TeamManagerId = command.TeamManagerId;
        existingTeam.MemberId = command.MemberId;
        await teamRepository.UpdateTeamAsync(existingTeam);
        return mapper.Map<TeamRequestDto>(existingTeam);
    }
}
