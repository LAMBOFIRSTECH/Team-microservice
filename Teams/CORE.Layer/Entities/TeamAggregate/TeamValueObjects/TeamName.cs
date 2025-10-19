using System.Text.RegularExpressions;
using Teams.API.Layer.Middlewares;

namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;

public sealed class TeamName : IEquatable<TeamName>
{
    public string Value { get; init; }
    public TeamName(string value)
    {
        if (value == string.Empty)
            throw new ArgumentException("Team name cannot be empty.", nameof(value));
        Value = value;
    }
    public static TeamName Create(string value)
    {

        if (!Regex.IsMatch(value, @"^[\p{L}\s\-']+$"))
            throw HandlerException.BadRequest(
                title: "Entry format",
                statusCode: 400,
                message: $"Team name [[{value}]] contains invalid characters.",
                reason: "Value Objet Validation Error"
            );
        return new TeamName(value.Trim());
    }
    public override string ToString() => Value;
    public override bool Equals(object obj) => obj is TeamName tn && Equals(tn);
    public override int GetHashCode() => Value.GetHashCode();
    public bool Equals(TeamName? other) => other != null && Value == other.Value;
    public static bool operator ==(TeamName? left, TeamName? right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(TeamName? left, TeamName? right) => !(left == right);
}