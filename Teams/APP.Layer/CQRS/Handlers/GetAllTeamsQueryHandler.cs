using MediatR;
using Teams.CORE.Layer.Interfaces;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Queries;
namespace Teams.APP.Layer.CQRS.Handlers;
// public class GetAllTeamsQueryHandler : IRequestHandler<GetAllTeamsQuery, TeamDto>
// {
//     private readonly ITeamRepository _teamRepository;
//     public GetAllTeamsQueryHandler(ITeamRepository teamRepository)
//     {
//         _teamRepository = teamRepository;
//     }

//     public async Task<TeamDto> Handle(GetAllTeamsQuery request, CancellationToken cancellationToken)
//     {
//         return await _teamRepository.GetAllTeamsAsync(cancellationToken);
//     }
// }

