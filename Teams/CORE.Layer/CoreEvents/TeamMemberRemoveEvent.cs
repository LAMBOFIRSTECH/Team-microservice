using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamMemberRemoveEvent(Guid teamId, Guid memberId) : IDomainEvent
{
   
}