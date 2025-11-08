using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Exceptions;
namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamExtensionMethods;

public static class TeamExtension
{
    private static readonly int _maturityPeriod = 30;  // En prod : >= 180 jours
    private static string _verdict = string.Empty;
    public static Dictionary<TeamState, string> StateMappings =>
       Enum.GetValues(typeof(TeamState))
           .Cast<TeamState>()
           .ToDictionary(state => state, state => _verdict);

    public static bool UseTeamArchivedState(this string state)
    {
        TeamState currentStatus;
        Enum.TryParse(state, out currentStatus);
        if (currentStatus != TeamState.Archived) return false;
        return true;
    }
    public static IEnumerable<Team> GetExpiredTeams(this IEnumerable<Team> teams) => teams.Where(t => t.IsTeamExpired()).ToList();
    public static IEnumerable<Team> GetMatureTeams(this IEnumerable<Team> teams)
        => teams.Where(t => (DateTimeOffset.Now - t.TeamCreationDate).TotalSeconds >= _maturityPeriod).ToList(); // 30 pour les tests refactoriser

    public static int CountMatureTeams(this IEnumerable<Team> teams)
        => teams.Count(t => (DateTimeOffset.Now - t.TeamCreationDate).TotalSeconds >= _maturityPeriod);

    public static int CountExpiredTeams(this IEnumerable<Team> teams)
        => teams.Count(t => t.IsTeamExpired());

    public static int CountActiveTeams(this IEnumerable<Team> teams)
        => teams.Count(t => !t.IsTeamExpired());

    public static int CountTeamsNearingExpiration(this IEnumerable<Team> teams)
    {
        return teams.Count(t =>
        {
            var timeToExpiration = t.Expiration - DateTimeOffset.Now;
            return timeToExpiration.TotalSeconds <= 15 && timeToExpiration.TotalSeconds > 0; // en prod : AddDays(30)
        });
    }
    public static int CountTeamsNearingMaturity(this IEnumerable<Team> teams)
    {
        return teams.Count(t =>
        {
            var timeToMaturity = t.TeamCreationDate.AddSeconds(_maturityPeriod) - DateTimeOffset.Now;
            return timeToMaturity.TotalSeconds <= 15 && timeToMaturity.TotalSeconds > 0; // en prod : AddDays(30)
        });
    }
    public static int CountArchivedTeams(this IEnumerable<Team> teams)
        => teams.Count(t => t.State == TeamState.Archived);

    public static IEnumerable<DateTimeOffset> GetfutureMaturities(this IEnumerable<Team> teams)
        => teams.Select(t => t.TeamCreationDate.AddSeconds(30))
                .Where(d => d > DateTimeOffset.Now)
                .ToList(); // en prod : AddDays(180)
    public static IEnumerable<DateTimeOffset> GetfutureExpirations(this IEnumerable<Team> teams)
        => teams.Where(t => t.Expiration > DateTimeOffset.Now)
                .Select(t => t.Expiration);
    public static IEnumerable<Team> ArchiveTeams(this IEnumerable<Team> teams)
    {
        foreach (var team in teams) team.ArchiveTeam(); return teams;
    }
    public static string MatureTeam(this Team team)
    {
        if (!team.IsMature())
        {
            _verdict = "not yet mature";
            _ = StateMappings[team.State];
            return $"✅ Team is {team.State} with {team.ProjectState} project however {StateMappings[team.State]}.";
        }
        _verdict = "Mature";
        _ = StateMappings[team.State];
        return $"✅ Team is {team.State} with {team.ProjectState} project and {StateMappings[team.State]}.";
    }
    public static async Task<Team> CreateTeamAsync(this string name, Guid teamManagerId, IEnumerable<Guid> memberIds, IEnumerable<Team> teams)
    {
        if (teams.Any(t => t.Name.Value.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException($"A team with the name already exists.", nameof(name));

        if (teams.Count(t => t.TeamManagerId.Value == teamManagerId) > 3)
            throw new BusinessRuleException("A manager cannot handle with more than 3 teams.");

        if (teams.Any(t => t.MembersIds.Count == memberIds.Count() && !t.MembersIds.Select(m => m.Value).Except(memberIds).Any() && t.TeamManagerId.Value == teamManagerId))
            throw new ConflictException("A team with exactly the same members and manager already exists.", nameof(name));

        if (GetCommonMembersStats(memberIds, teams) >= 50)
            throw new BusinessRuleException("Cannot create a team with more than 50% common members with existing team.");

        return Team.Create(name, teamManagerId, memberIds);
    }
    /// <summary>
    /// Calculates the maximum percentage of common members between a new team and a collection of existing teams.
    /// </summary>
    /// <param name="newTeamMembers">The list of members (as <see cref="Guid"/>) for the new team being created.</param>
    /// <param name="existingTeams">The collection of existing teams to compare against.</param>
    /// <returns>
    /// A <see cref="double"/> representing the highest percentage of overlap in members
    /// between the new team and any existing team. Returns 0 if no existing teams are provided.
    /// </returns>
    /// <exception cref="BusinessRuleException">
    /// Thrown when <paramref name="newTeamMembers"/> is null or contains fewer than two members.
    /// </exception>
    private static double GetCommonMembersStats(IEnumerable<Guid> newTeamMembers, IEnumerable<Team> existingTeams)
    {
        if (newTeamMembers == null || newTeamMembers.Count() == 0)
            throw new BusinessRuleException("The new team must have at least three members.");

        if (existingTeams == null || existingTeams.Count() == 0) return 0;
        double maxPercent = 0;
        foreach (var team in existingTeams)
        {
            var common = team.MembersIds.Select(m => m.Value).Intersect(newTeamMembers).Count();
            var universe = team.MembersIds.Select(m => m.Value).Union(newTeamMembers).Count();
            double percent = (double)common / universe * 100;
            if (percent > maxPercent) maxPercent = percent;
        }
        return maxPercent;
    }
}