using System.Text.RegularExpressions;
using Teams.API.Layer.Middlewares;

namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;

public class TeamName
{
    public string Value { get; private set; }
    private TeamName(string value) => Value = value;
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
}