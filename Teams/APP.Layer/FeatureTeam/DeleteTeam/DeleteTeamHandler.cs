using MediatR;
using Microsoft.Extensions.Logging;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.APP.Layer.FeatureTeam.DeleteTeam;
public sealed class DeleteTeamHandler(IUnitOfWork _unitOfWork, ILogger<DeleteTeamHandler> _log, ITeamProjectLifeCycle _teamProjectLifeCycle): IRequestHandler<DeleteTeamCommand>
{
    public async Task Handle(DeleteTeamCommand command, CancellationToken cancellationToken)
    {
        await _teamProjectLifeCycle.DeleteTeamProjectAsync(cancellationToken, command.Id);
        await _unitOfWork.CommitAsync(cancellationToken);
        LogHelper.Info($"✅ Team has been deleted successfully.", _log);
    }
}

