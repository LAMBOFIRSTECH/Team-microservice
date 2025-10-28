using NodaTime;
using Teams.CORE.Layer.CoreInterfaces;
using Teams.CORE.Layer.Entities.GeneralValueObjects;

namespace Teams.CORE.Layer.CoreEvents;
public record TeamDeletedEvent(Guid TeamId, string TeamName, DateTimeOffset DeletionDate, Guid EventId)  : IDomainEvent
{
    public LocalizationDateTime OccurredOn { get; init; } = LocalizationDateTime.FromDateTimeUtc(DateTime.UtcNow);
}
