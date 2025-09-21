using System.Text.RegularExpressions;
using Teams.API.Layer.Middlewares;

namespace Teams.CORE.Layer.ValueObjects;

public class TeamName
{
    public string Value { get; private set; }

    private TeamName(string value)
    {
        Value = value;
    }

    public static TeamName Create(string value)
    {

        if (!Regex.IsMatch(value, @"^[\p{L}\s\-']+$"))
            throw new HandlerException(
                   500,
                   "Team name contains invalid characters.",
                   "Entry format",
                   "Data validation error"
               );
        return new TeamName(value.Trim());
    }

    public override string ToString() => Value;
}
