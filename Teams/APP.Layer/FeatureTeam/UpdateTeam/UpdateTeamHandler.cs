using MediatR;
using Teams.APP.Layer.Exceptions;
using Teams.CORE.Layer.CoreInterfaces;
using Microsoft.Extensions.Logging;

namespace Teams.APP.Layer.FeatureTeam.UpdateTeam;
public sealed class UpdateTeamHandler(IUnitOfWork _unitOfWork, ILogger<UpdateTeamHandler> _log) : IRequestHandler<UpdateTeamCommand>
{
    public async Task Handle(UpdateTeamCommand command, CancellationToken cancellationToken)
    {
        var existingTeam = await _unitOfWork.TeamRepository.GetByIdAsync(command.Id, cancellationToken);
        if (existingTeam == null)
        {
            throw new HandlerException(
                404,
                $"A team with the Id '{command.Id}' not found.",
                "Not Found",
                "Team ID not found"
            );
        }
        existingTeam.UpdateTeam(command.Name!, command.TeamManagerId, command.MembersIds);
        _unitOfWork.TeamRepository.Update(existingTeam);
        await _unitOfWork.CommitAsync(cancellationToken);
        _log.LogInformation($"✅ Team {existingTeam.Name} has been updated successfully.");
    }
}
