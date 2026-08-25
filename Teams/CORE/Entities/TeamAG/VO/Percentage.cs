namespace Teams.CORE.Entities.TeamAG.VO;

public sealed class Percentage : IEquatable<Percentage>
{
    public double Value { get; }

    public Percentage(double value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException("Percentage must be between 0 and 100.", nameof(value));
        Value = value;
    }

    public bool Equals(Percentage? other) => other != null && Value.Equals(other.Value);
    public override bool Equals(object? obj) => obj is Percentage other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => $"{Value}%";
}