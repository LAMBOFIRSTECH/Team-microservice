using Teams.CORE.Layer.CoreInterfaces;
namespace Teams.CORE.Layer.CoreEvents.TeamEvents;
public record TeamMemberRemoveEvent(Guid teamId, Guid memberId) : IDomainEvent
{
   
}