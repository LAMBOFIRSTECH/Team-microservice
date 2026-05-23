using MediatR;
using Teams.APP.Layer.DTOs.Output;

namespace Teams.APP.Layer.FeatureTeam.GetAllTeams;

// Ce handler ne connaît absolument pas l'existence d'EF Core ou de l'INFRA
public sealed class GetAllTeamsHandler(IUnitOfWork _unitOfWork, IMapper _mapper) : IRequestHandler<GetAllTeamsQuery, List<TeamDtoModels.TeamDto>>
{
    public async Task<List<TeamDtoModels.TeamDto>> Handle(GetAllTeamsQuery request, CancellationToken ct)
    {
        // On passe par l'interface abstraite définie dans le Core
        var teams = await _unitOfWork.TeamRepository.GetAllAsync(ct);
        return _mapper.Map<List<TeamDtoModels.TeamDto>>(teams);
    }
}