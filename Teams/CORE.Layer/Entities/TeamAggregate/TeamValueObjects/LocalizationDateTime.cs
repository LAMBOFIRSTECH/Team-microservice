using NodaTime;

namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects
{
    public sealed class LocalizationDateTime : IEquatable<LocalizationDateTime>
    {
        private static readonly DateTimeZone zoneId = DateTimeZoneProviders.Tzdb["Europe/Paris"];
        public ZonedDateTime Value { get; }
        private LocalizationDateTime(ZonedDateTime value) => Value = value;
        private LocalizationDateTime() { }
        public static LocalizationDateTime FromInstant(Instant instant)
            => new LocalizationDateTime(instant.InZone(zoneId));

        public static LocalizationDateTime FromDateTimeUtc(DateTime dateTimeUtc)
            => FromInstant(Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTimeUtc, DateTimeKind.Utc)));

        public DateTime ToDateTimeUtc() => Value.ToDateTimeUtc();
        public static LocalizationDateTime Now(IClock clock)
            => new LocalizationDateTime(clock.GetCurrentInstant().InZone(zoneId));

        public LocalizationDateTime Plus(Duration duration)
            => new LocalizationDateTime(Value.Plus(duration));

        public Instant ToInstant() => Value.ToInstant();
        public bool Equals(LocalizationDateTime? other)
            => other is not null && Value.ToInstant() == other.Value.ToInstant();
        public override bool Equals(object? obj) => Equals(obj as LocalizationDateTime);
        public override int GetHashCode() => Value.ToInstant().GetHashCode();
        public override string ToString() => Value.ToString();
    }
}
