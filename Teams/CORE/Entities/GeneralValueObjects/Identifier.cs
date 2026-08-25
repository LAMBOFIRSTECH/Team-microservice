namespace Teams.CORE.Entities.GeneralValueObjects;
public abstract class Identifier<T> : IEquatable<Identifier<T>> where T : Identifier<T>
{
    public Guid Value { get; }

    protected Identifier(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Identifier cannot be empty.", nameof(value));
        Value = value;
    }

    public bool Equals(Identifier<T>? other)
    {
        return other != null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Identifier<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(Identifier<T>? left, Identifier<T>? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Identifier<T>? left, Identifier<T>? right) => !(left == right);

    public override string ToString()
    {
        return Value.ToString();
    }
}
