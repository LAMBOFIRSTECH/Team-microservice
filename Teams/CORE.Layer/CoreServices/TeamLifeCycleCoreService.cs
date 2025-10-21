using NodaTime;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities.TeamAggregate;

namespace Teams.CORE.Layer.CoreServices;

public class TeamLifeCycleCoreService
{
    private string _verdict = string.Empty;
    public Dictionary<TeamState, string> StateMappings =>
       Enum.GetValues(typeof(TeamState))
           .Cast<TeamState>()
           .ToDictionary(state => state, state => _verdict);
    private readonly int _maturityPeriod = 30;  // En prod : >= 180 jours
    public IEnumerable<Team> GetExpiredTeams(IEnumerable<Team> teams) => teams.Where(t => t.IsTeamExpired()).ToList();
    public IEnumerable<Team> GetMatureTeams(IEnumerable<Team> teams)
    {
        Console.WriteLine($"voici le temps {SystemClock.Instance.GetCurrentInstant()}");
        return teams.Where(t => (SystemClock.Instance.GetCurrentInstant() - t.TeamCreationDate.Value.ToInstant()).TotalSeconds >= _maturityPeriod)
               .ToList(); // 30 pour les tests refactoriser
    }


    public IEnumerable<Instant> GetfutureMaturities(IEnumerable<Team> teams)
        => teams.Select(t => t.TeamCreationDate.Plus(Duration.FromSeconds(30)).ToInstant())
                .Where(d => d > SystemClock.Instance.GetCurrentInstant())
                .ToList(); // en prod : AddDays(180)
    public IEnumerable<Instant> GetfutureExpirations(IEnumerable<Team> teams)
        => teams.Where(t => t.Expiration.ToInstant() > SystemClock.Instance.GetCurrentInstant())
                .Select(t => t.Expiration.ToInstant());
    public void ArchiveTeams(IEnumerable<Team> teams)
    {
        foreach (var team in teams) team.ArchiveTeam();
    }

    public string MatureTeam(Team team)
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
    public async Task<Team> CreateTeamAsync(string name, Guid teamManagerId, IEnumerable<Guid> memberIds, IEnumerable<Team> teams)
    {
        if (teams.Any(t => t.Name.Value.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"A team with the name '{name}' already exists.");

        if (teams.Count(t => t.TeamManagerId.Value == teamManagerId) > 3)
            throw new DomainException("A manager cannot handle with more than 3 teams.");

        if (teams.Any(t => t.MembersIds.Count == memberIds.Count() && !t.MembersIds.Select(m => m.Value).Except(memberIds).Any() && t.TeamManagerId.Value == teamManagerId))
            throw new DomainException("A team with exactly the same members and manager already exists.");

        if (GetCommonMembersStats(memberIds, teams) >= 50)
            throw new DomainException("Cannot create a team with more than 50% common members with existing team.");

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
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="newTeamMembers"/> is null or contains fewer than two members.
    /// </exception>
    private double GetCommonMembersStats(IEnumerable<Guid> newTeamMembers, IEnumerable<Team> existingTeams)
    {
        if (newTeamMembers == null || newTeamMembers.Count() == 0)
            throw new DomainException("The new team must have at least three members.");

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

