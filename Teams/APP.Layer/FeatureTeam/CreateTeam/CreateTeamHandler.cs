using MediatR;
using Teams.APP.Layer.Helpers;
using Teams.CORE.Layer.CoreInterfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
namespace Teams.APP.Layer.FeatureTeam.CreateTeam;
public sealed class CreateTeamHandler(IUnitOfWork _unitOfWork, ILogger<CreateTeamHandler> _log, ITeamCreationService _teamCreationService) : IRequestHandler<CreateTeamCommand, CreateTeamResponse>
{
    public async Task<CreateTeamResponse> Handle(CreateTeamCommand command, CancellationToken cancellationToken)
    {
        var team = await _teamCreationService.CreateUniqueTeamAsync(command.Name, command.TeamManagerId, command.MembersIds, cancellationToken);
        await _unitOfWork.TeamRepository.AddAsync(team, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        LogHelper.Info($"✅ Team {team.Name} has been created successfully.", _log);
        return new CreateTeamResponse(team.Id, team.Name.Value, team.TeamManagerId.Value, team.MembersIds.Select(x => x.Value).ToImmutableArray());
    }
}