using Teams.CORE.Layer.CoreInterfaces;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamCreatedEvent(Guid teamId) : IDomainEvent
{
    public LocalizationDateTime OccurredOn { get; init; } = LocalizationDateTime.FromDateTimeUtc(DateTime.UtcNow);
}