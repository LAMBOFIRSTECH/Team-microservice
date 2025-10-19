using NodaTime;

namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects
{
    public sealed class LocalizationDateTime : IEquatable<LocalizationDateTime>
    {
        private static readonly DateTimeZone zoneId = 
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Paris") ?? DateTimeZone.Utc;

        public static LocalizationDateTime MinValue => FromInstant(Instant.FromUnixTimeSeconds(0));
        public static LocalizationDateTime MaxValue => FromInstant(Instant.FromUtc(9999, 12, 31, 23, 59, 59));

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

        public static bool operator ==(LocalizationDateTime? left, LocalizationDateTime? right) 
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(LocalizationDateTime? left, LocalizationDateTime? right) 
            => !(left == right);

        public override int GetHashCode() => Value.ToInstant().GetHashCode();
        public override string ToString() => Value.ToString();
    }
}
