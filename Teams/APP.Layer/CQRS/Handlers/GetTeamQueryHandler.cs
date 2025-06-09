using MediatR;
using Teams.CORE.Layer.Interfaces;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Queries;
using AutoMapper;
namespace Teams.APP.Layer.CQRS.Handlers;

public class GetTeamQueryHandler : IRequestHandler<GetTeamQuery, TeamDto>
{
    private readonly ITeamRepository teamRepository;
    private readonly IMapper mapper;
    public GetTeamQueryHandler(ITeamRepository teamRepository, IMapper mapper)
    {
        this.teamRepository = teamRepository;
        this.mapper = mapper;
    }

    public async Task<TeamDto> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await teamRepository.GetTeamByIdAsync(request.Id)!
        ?? throw new HandlerException
        (
            404,
            $"Team with ID {request.Id} not found.",
            "Not Found",
            "Team ressource not found"
        );
        var teamDto = mapper.Map<TeamDto>(team);
        return teamDto;
    }

}