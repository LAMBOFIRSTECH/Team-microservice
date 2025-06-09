using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;
using AutoMapper;
namespace Teams.APP.Layer.CQRS.Handlers;

public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, TeamDto>
{
    private readonly ITeamRepository teamRepository;
    private readonly IMapper mapper;
    public CreateTeamCommandHandler(ITeamRepository teamRepository, IMapper mapper)
    {
        this.teamRepository = teamRepository;
        this.mapper = mapper;
    }
    public async Task<TeamDto> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var listOfTeams = await teamRepository.GetAllTeamsAsync();
        if (listOfTeams.Any(t => t.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new HandlerException(
                409,
                $"A team with the name '{request.Name}' already exists.",
                "Conflict",
                "Team Name Conflict"

            );
        }
        var team = mapper.Map<Team>(request);
        await teamRepository.CreateTeamAsync(team);
        return mapper.Map<TeamDto>(team);
    }
}
