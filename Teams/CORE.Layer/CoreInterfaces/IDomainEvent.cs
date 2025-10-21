using Teams.CORE.Layer.Entities.GeneralValueObjects;

namespace Teams.CORE.Layer.CoreInterfaces;

public interface IDomainEvent
{
    LocalizationDateTime OccurredOn { get; }
}
