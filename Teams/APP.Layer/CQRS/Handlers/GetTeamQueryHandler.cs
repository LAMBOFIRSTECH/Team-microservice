using MediatR;
using Teams.CORE.Layer.Interfaces;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Queries;
namespace Teams.APP.Layer.CQRS.Handlers;
// public class GetTeamQueryHandler : IRequestHandler<GetTeamQuery, TeamDto>
// {
//     private readonly ITeamRepository _teamRepository;
//     public GetTeamQueryHandler(ITeamRepository teamRepository)
//     {
//         _teamRepository = teamRepository;
//     }

//     public async Task<TeamDto> Handle(GetTeamQuery request, CancellationToken cancellationToken)
//     {
//         return await _teamRepository.GetTeamByIdAsync(request.Id);
//     }
// }

