using Teams.CORE.CoreInterfaces;
using Teams.CORE.Entities.GeneralValueObjects;
using Teams.CORE.Entities.TeamAG.VO;
namespace Teams.CORE.CoreEvents;
public sealed record TeamEventsDeadLetter : IDomainEvent
{
    public Guid Id { get; set; }

    public TeamId? TeamId { get; set; }

    public StringValue EventType { get; set; } = null!;

    public StringValue PayloadJson { get; set; } = null!;

    public StringValue Exchange { get; set; } = null!;

    public StringValue RoutingKey { get; set; } = null!;

    public Guid? CorrelationId { get; set; }

    public DateTime OccurredOn { get; set; }

    public int RetryCount { get; set; }

    public StringValue? LastError { get; set; }

    public DateTime DeadLetteredAt { get; set; }
}