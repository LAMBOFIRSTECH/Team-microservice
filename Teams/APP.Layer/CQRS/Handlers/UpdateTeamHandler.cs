using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.CORE.Layer.Exceptions;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class UpdateTeamHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
    : IRequestHandler<UpdateTeamCommand, TeamRequestDto>
{
    public async Task<TeamRequestDto> Handle(UpdateTeamCommand command, CancellationToken cancellationToken)
    {
        var existingTeam = await _unitOfWork.TeamRepository.GetById(cancellationToken, command.Id);
        if (existingTeam == null)
        {
            throw new HandlerException(
                404,
                $"A team with the Id '{command.Id}' not found.",
                "Not Found",
                "Team ID not found"
            );
        }
        try
        {
            existingTeam.UpdateTeam(command.Name!, command.TeamManagerId, command.MemberId);
        }
        catch (DomainException ex)
        {
            throw HandlerException.BadRequest(ex.Message, "Validation Error");
        }
        _unitOfWork.TeamRepository.Update(existingTeam);
        return _mapper.Map<TeamRequestDto>(existingTeam);
    }
}
