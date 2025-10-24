using MediatR;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.Helpers;
using Teams.APP.Layer.Interfaces;
using Teams.INFRA.Layer.Interfaces;

namespace Teams.APP.Layer.CQRS.Handlers;

public class DeleteTeamHandler(IUnitOfWork _unitOfWork, ILogger<DeleteTeamHandler> _log, ITeamProjectLifeCycle _teamProjectLifeCycle)
    : IRequestHandler<DeleteTeamCommand>
{
    public async Task Handle(DeleteTeamCommand command, CancellationToken cancellationToken)
    {
        await _teamProjectLifeCycle.DeleteTeamProjectAsync(cancellationToken, command.Id);
        await _unitOfWork.SaveAsync(cancellationToken);
        LogHelper.Info($"✅ Team has been deleted successfully.", _log);
    }
}

