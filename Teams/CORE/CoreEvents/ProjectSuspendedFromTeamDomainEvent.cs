using Teams.CORE.CoreInterfaces;
namespace Teams.CORE.CoreEvents;

public record ProjectSuspendedFromTeamDomainEvent(Guid ProjectId,string ProjectName,Guid TeamManagerId,DateTimeOffset OccurredOn) : IDomainEvent
{
}