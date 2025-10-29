using AutoMapper;
using MediatR;
using Teams.API.Layer.DTOs;
using Teams.API.Layer.Middlewares;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.INFRA.Layer.Interfaces;
using Teams.CORE.Layer.CommonExtensions;

namespace Teams.APP.Layer.CQRS.Handlers;
public class CreateTeamHandler(IUnitOfWork _unitOfWork, IMapper _mapper, ILogger<CreateTeamHandler> _log) : IRequestHandler<CreateTeamCommand, TeamDto>
{
    public async Task<TeamDto> Handle(CreateTeamCommand cmd, CancellationToken ct)
    {
        var existingTeams = _unitOfWork.TeamRepository.GetAll();
        try
        {
            var team = await cmd.Name.CreateTeamAsync(cmd.TeamManagerId, cmd.MembersIds, existingTeams);
            await _unitOfWork.TeamRepository.Create(team, ct);
            await _unitOfWork.SaveAsync(ct);
            LogHelper.Info($"✅ Team {team.Name} has been created successfully.", _log);
            return _mapper.Map<TeamDto>(team);
        }
        catch (DomainException ex)
        {
            LogHelper.BusinessRuleFailure(_log, "Team creation failed", ex.Message, null);
            throw HandlerException.DomainError(title: "Team creation failed", statusCode: 422, message: ex.Message, reason: "Domain Validation Error");
        }
    }
}