using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Services;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class CreateTeamCommandHandler(
    ITeamRepository teamRepository,
    IMapper mapper,
    EmployeService employeService
) : IRequestHandler<CreateTeamCommand, TeamDto>
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

    public async Task<TeamDto> Handle(
        CreateTeamCommand command,
        CancellationToken cancellationToken
    )
    {
        var listOfTeams = await teamRepository.GetAllTeamsAsync();
        var uniqueMemberIds = command.MemberId.Distinct().ToList();
        if (command.MemberId.Count < 2)
        {
            throw new HandlerException(
                400,
                "A team must have at least 2 members, please add more members.",
                "Bad Request",
                "Not Enough Members"
            );
        }
        if (command.MemberId.Count > 10)
        {
            throw new HandlerException(
                500,
                "A team cannot have more than 10 members, please reduce the number of members.",
                "Internal Server Error",
                "Too Many Members"
            );
        }
        if (uniqueMemberIds.Count != command.MemberId.Count)
        {
            throw new HandlerException(
                400,
                $"Team members must be unique, please remove duplicates.",
                "Bad Request",
                "Duplicate Members"
            );
        }
        if (!uniqueMemberIds.Contains(command.TeamManagerId))
        {
            throw new HandlerException(
                400,
                "The team manager must be one of the team members.",
                "Bad Request",
                "Manager Not in Members"
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
        if (listOfTeams.Any(t => t.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new HandlerException(
                409,
                $"A team with the name '{command.Name}' already exists.",
                "Conflict",
                "Team Name Conflict"
            );
        }
        var team = mapper.Map<Team>(command);
        await teamRepository.CreateTeamAsync(team);
        return mapper.Map<TeamDto>(team);
    }
}
