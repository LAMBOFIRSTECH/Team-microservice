using System.Text.RegularExpressions;

namespace Teams.CORE.Entities.GeneralValueObjects;

public sealed class StringValue : IEquatable<StringValue>
{
    public string Value { get; init; }
    public StringValue(string value)
    {
        if (value == string.Empty)
            throw new ArgumentException("Objet cannot be empty.", nameof(value));
        Value = value;
    }
    public static StringValue Create(string value)
    {

        if (!Regex.IsMatch(value, @"^[\p{L}\s\-']+$"))
            throw new ArgumentException($"Object [[{value}]] contains invalid characters.Value Objet Validation Error", nameof(value)
            );
        return new StringValue(value.Trim());
    }
    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is StringValue sv && Equals(sv);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public bool Equals(StringValue? other) => other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);
    public static bool operator ==(StringValue? left, StringValue? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(StringValue? left, StringValue? right) => !(left == right);
}