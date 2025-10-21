using Teams.CORE.Layer.CoreInterfaces;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamMaturityEvent(Guid teamId) : IDomainEvent
{
    public LocalizationDateTime OccurredOn { get; init; } = LocalizationDateTime.FromDateTimeUtc(DateTime.UtcNow);
}
