using MediatR;
using Teams.CORE.Layer.CoreInterfaces;
using AutoMapper;
using Teams.APP.Layer.DTOs.Output;

namespace Teams.APP.Layer.FeatureTeam.GetAllTeams;
public class GetAllTeamsHandler(IUnitOfWork _unitOfWork, IMapper _mapper) 
    : IRequestHandler<GetAllTeamsQuery, List<TeamDtoModels.TeamDto>>
{
    public async Task<List<TeamDtoModels.TeamDto>> Handle(GetAllTeamsQuery request, CancellationToken ct)
    {
        
        var teams = await _unitOfWork.TeamRepository.GetAllAsync(request.PageNumber, request.PageSize, ct);
        return _mapper.Map<List<TeamDtoModels.TeamDto>>(teams);
    }
}