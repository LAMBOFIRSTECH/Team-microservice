namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;
[Microsoft.EntityFrameworkCore.Owned]
public sealed class MemberId : IEquatable<MemberId>
{
    public Guid Value { get; init; }
    public MemberId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MemberId cannot be empty.", nameof(value));
        Value = value;
    }
    public override bool Equals(object obj)
    => obj is MemberId other && Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public bool Equals(MemberId? other) => other != null && Value == other.Value;

    public static bool operator ==(MemberId? left, MemberId? right)
    => left is null ? right is null : left.Equals(right);

    public static bool operator !=(MemberId? left, MemberId? right) => !(left == right);
    public override string ToString() => Value.ToString();

}
