using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Services;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class CreateTeamCommandHandler(ITeamRepository teamRepository, IMapper mapper)
    : IRequestHandler<CreateTeamCommand, TeamDto>
{
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
