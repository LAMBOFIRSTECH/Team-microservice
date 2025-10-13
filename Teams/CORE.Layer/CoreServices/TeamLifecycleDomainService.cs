using NodaTime;
using Teams.CORE.Layer.Entities.TeamAggregate;

namespace Teams.CORE.Layer.CoreServices;

public class TeamLifecycleDomainService
{
    private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
    private readonly TimeSpan _maturityPeriod = TimeSpan.FromSeconds(30);  // En prod : >= 180 jours
    public IEnumerable<Team> GetExpiredTeams(IEnumerable<Team> teams) => teams.Where(t => t.IsTeamExpired()).ToList();
    public IEnumerable<Team> GetMatureTeams(IEnumerable<Team> teams) => teams.Where(t => (now - t.TeamCreationDate.Value.ToInstant()).TotalSeconds >= 30).ToList();

    public IEnumerable<Instant> GetfutureMaturities(IEnumerable<Team> teams)
        => teams.Select(t => t.TeamCreationDate.Plus(Duration.FromSeconds(30)).ToInstant())
                .Where(d => d >  SystemClock.Instance.GetCurrentInstant())
                .ToList(); // en prod : AddDays(180)
    public IEnumerable<Instant> GetfutureExpirations(IEnumerable<Team> teams)
        => teams.Where(t => t.Expiration.ToInstant() >  SystemClock.Instance.GetCurrentInstant())
                .Select(t => t.Expiration.ToInstant());
    public void ArchiveTeams(IEnumerable<Team> teams)
    {
        foreach (var team in teams) team.ArchiveTeam();
    }
    public void MatureTeams(IEnumerable<Team> teams)
    {
        foreach (var team in teams) team.IsMature();
    }
}

