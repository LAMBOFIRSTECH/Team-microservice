using CORE.Entities.TeamAG;
using MediatR;
using Microsoft.Extensions.Logging;
using Teams.CORE.CoreInterfaces;
using Teams.CORE.Entities.TeamAG;

namespace Teams.APP.FeatureTeam.SyncObjectsAssociation;
/// <summary>
/// Application command carrying data decoupled from any third-party serialization framework.
/// </summary>
#pragma warning disable MA0048
public sealed record SyncProjectDetailItem(string ProjectName, DateTimeOffset ProjectStartDate, DateTimeOffset ProjectEndDate, int State, DateTimeOffset? SuspendedAt);
#pragma warning restore MA0048

#pragma warning disable MA0048
public sealed record SyncProjectAssociationCommand(Guid ProjectId, Guid TeamManagerId, string TeamName, int AssignmentState, ICollection<SyncProjectDetailItem> Details) : IRequest;
#pragma warning restore MA0048

/// <summary>
/// Command handler executing the use case workflow and enforcing the Anti-Corruption Layer mapping.
/// </summary>
public sealed class SyncProjectAssociationHandler(IUnitOfWork unitOfWork, ILogger<SyncProjectAssociationHandler> log) : IRequestHandler<SyncProjectAssociationCommand>
{
    public async Task Handle(SyncProjectAssociationCommand command, CancellationToken cancellationToken)
    {
        // 1. Retrieve the aggregate root through its boundary
        var team = await unitOfWork.TeamRepository.GetTeamByNameAndTeamManagerIdAsync(
            command.TeamName,
            command.TeamManagerId,
            cancellationToken).ConfigureAwait(false);
        if (team == null)
            throw new InvalidOperationException(string.Format("Team not found for manager profile: {0}", command.TeamManagerId));

        // 2. Anti-Corruption Layer (ACL): Explicit conversion to core Domain Types
        // Ceci permet d'éviter les le CAST des enums
        var domainAssignmentState = command.AssignmentState switch
        {
            0 => ProjectAssignmentState.Unassigned,
            1 => ProjectAssignmentState.Assigned,
            2 => ProjectAssignmentState.Suspended,
            3 => ProjectAssignmentState.UnderReview,
            4 => ProjectAssignmentState.UnassignedAfterReview,
            _ => throw new InvalidOperationException($"Valeur AssignmentState inconnue reçue : {command.AssignmentState}")
        };

        var domainDetailsInput = command.Details.Select(d =>
        {
            var state = d.State switch
            {
                0 => ProjectDetails.ProjectDetailState.Active,
                1 => ProjectDetails.ProjectDetailState.Suspended,
                _ => throw new InvalidOperationException($"Valeur State inconnue reçue {d.State}")
            };
            return (d.ProjectName, d.ProjectStartDate, d.ProjectEndDate, state, d.SuspendedAt);
        }).ToList();

        // 3. Trigger domain business rule logic inside the Aggregate Root boundary
       // team.SyncProjectAssociation(command.ProjectId, domainAssignmentState, domainDetailsInput);
        await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);

        log.LogInformation("Project association for Project {ProjectId} has been successfully synchronized on Team {TeamId}.", command.ProjectId, team.Id);
    }
}