using Teams.CORE.Layer.Entities;

namespace Teams.CORE.Layer.CoreServices;

public class TeamLifecycleDomainService
{
    private readonly TimeSpan _maturityPeriod = TimeSpan.FromSeconds(30);  // En prod : >= 180 jours
    public IEnumerable<Team> GetExpiredTeams(IEnumerable<Team> teams) => teams.Where(t => t.IsTeamExpired()).ToList();
    public IEnumerable<Team> GetMatureTeams(IEnumerable<Team> teams) => teams.Where(t => (DateTime.Now - t.TeamCreationDate).TotalSeconds >= 30).ToList();
    public IEnumerable<DateTime> GetfutureMaturities(IEnumerable<Team> teams) => teams.Select(t => t.TeamCreationDate.AddSeconds(30)).Where(d => d > DateTime.Now); // en prod : AddDays(180)
    public IEnumerable<DateTime> GetfutureExpirations(IEnumerable<Team> teams) => teams.Where(t => t.ExpirationDate > DateTime.Now).Select(t => t.ExpirationDate);
    public void ArchiveTeams(IEnumerable<Team> teams) 
    {
        foreach (var team in teams) team.ArchiveTeam();
    }
    public void MatureTeams(IEnumerable<Team> teams)
    {
        foreach (var team in teams) team.IsMature();
    }
}

