using NodaTime;

namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects
{
    public sealed class EuropeanDate : IEquatable<EuropeanDate>
    {
        private static readonly DateTimeZone zoneId = DateTimeZoneProviders.Tzdb["Europe/Paris"];

        public ZonedDateTime Value { get; }

        private EuropeanDate(ZonedDateTime value) => Value = value;
        private EuropeanDate() { }

        public static EuropeanDate FromInstant(Instant instant)
            => new EuropeanDate(instant.InZone(zoneId));

        public static EuropeanDate FromDateTimeUtc(DateTime dateTimeUtc)
            => FromInstant(Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc)));

        public DateTime ToDateTimeUtc() => Value.ToDateTimeUtc();

        public static EuropeanDate Now(IClock clock)
            => new EuropeanDate(clock.GetCurrentInstant().InZone(zoneId));

        public EuropeanDate Plus(Duration duration)
            => new EuropeanDate(Value.Plus(duration));

        public Instant ToInstant() => Value.ToInstant();

        public bool Equals(EuropeanDate? other)
            => other is not null && Value.ToInstant() == other.Value.ToInstant();

        public override bool Equals(object? obj) => Equals(obj as EuropeanDate);

        public override int GetHashCode() => Value.ToInstant().GetHashCode();

        public override string ToString() => Value.ToString();
    }
}
