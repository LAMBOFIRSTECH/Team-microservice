namespace Teams.CORE.Entities.TeamAG.VO;

public sealed class ExtraDays : IEquatable<ExtraDays>
{
    public int Value { get; }

    public ExtraDays(int value)
    {
        if (value < 0)
            throw new ArgumentException("Extra days cannot be negative.", nameof(value));
        Value = value;
    }

    public bool Equals(ExtraDays? other) => other != null && Value == other.Value;
    public override bool Equals(object? obj) => obj is ExtraDays other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString();
}
