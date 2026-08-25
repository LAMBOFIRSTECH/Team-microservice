
using MediatR;
using Teams.INFRA.Messaging.DTOs;
using Teams.APP.FeatureTeam.SyncObjectsAssociation;

namespace Teams.INFRA.Messaging.Consumers;
/// <summary>
/// Infrastructure consumer for RabbitMQ messages.
/// Receives the raw external DTO and dispatches a clean command to the application layer.
/// </summary>
public sealed class ProjectAssociationConsumer(ISender mediator)
{
    public async Task ConsumeAsync(ProjectAssociationDto dto)
    {
        // Infrastructure maps its raw contract into the Application Command contract.
        // We pass the external enums as integers to completely decouple the Application Layer from Infra types.
        var command = new SyncProjectAssociationCommand(
            ProjectId: dto.ProjectId,
            TeamManagerId: dto.TeamManagerId,
            TeamName: dto.TeamName,
            AssignmentState: (int)dto.AssignmentState,
            Details: dto.Details.Select(d => new SyncProjectDetailItem(
                d.ProjectName,
                d.ProjectStartDate,
                d.ProjectEndDate,
                (int)d.State,
                d.SuspendedAt
            )).ToList()
        );

        await mediator.Send(command).ConfigureAwait(false);
    }
}