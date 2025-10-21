using Teams.CORE.Layer.CoreInterfaces;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamMemberRemoveEvent(Guid teamId, Guid memberId) : IDomainEvent
{
    public LocalizationDateTime OccurredOn { get; init; } = LocalizationDateTime.FromDateTimeUtc(DateTime.UtcNow);
}