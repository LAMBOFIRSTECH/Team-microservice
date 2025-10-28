using NodaTime;
using Teams.CORE.Layer.CoreInterfaces;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
namespace Teams.CORE.Layer.CoreEvents;

public record TeamArchiveEvent(Guid TeamId, string TeamName, DateTimeOffset ArchivedAt, Guid EventId)  : IDomainEvent
{
    public LocalizationDateTime OccurredOn { get; init; } = LocalizationDateTime.FromDateTimeUtc(DateTime.UtcNow);
}
