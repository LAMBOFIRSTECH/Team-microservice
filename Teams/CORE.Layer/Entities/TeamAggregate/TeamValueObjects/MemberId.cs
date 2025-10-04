namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;

[Microsoft.EntityFrameworkCore.Owned]
public class MemberId
{
    public Guid Value { get; private set; }
    public MemberId(Guid value) => Value = value;
    public override bool Equals(object obj)
    => obj is MemberId other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(MemberId a, MemberId b) => a?.Equals(b) ?? b is null;
    public static bool operator !=(MemberId a, MemberId b) => !(a == b);
}
