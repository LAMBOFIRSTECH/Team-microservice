using System;
using System.Text.RegularExpressions;

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
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Team name cannot be empty.");

        if (!Regex.IsMatch(value, @"^[\p{L}\s\-']+$"))
            throw new ArgumentException("Team name contains invalid characters.");

        return new TeamName(value.Trim());
    }

    public override string ToString() => Value;
}
