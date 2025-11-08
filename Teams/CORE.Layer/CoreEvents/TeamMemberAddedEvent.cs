using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamMemberAddedEvent(Guid teamId, Guid memberId) : IDomainEvent
{
    
}
