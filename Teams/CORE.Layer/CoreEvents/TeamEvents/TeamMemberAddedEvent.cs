using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents.TeamEvents;

public record TeamMemberAddedEvent(Guid teamId, Guid memberId) : IDomainEvent
{
    
}
