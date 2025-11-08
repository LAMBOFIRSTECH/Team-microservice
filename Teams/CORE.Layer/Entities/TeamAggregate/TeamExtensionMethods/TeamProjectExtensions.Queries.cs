using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Microsoft.EntityFrameworkCore;
using NodatimePackage.Classes;
using Teams.CORE.Layer.Exceptions;
namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamExtensionMethods;

public static partial class TeamProjectExtensions
{
    public static DateTimeOffset ConvertDatetimeIntoDateTimeOffset(this string timeZoneId, DateTimeOffset utcDateTimeOffset)
    {
        // Récupère le décalage exact du fuseau à la date donnée
        var (offset, _) = TimeOperations.GetTimeZoneOffsetWithDaylight(timeZoneId, utcDateTimeOffset.UtcDateTime);
        return utcDateTimeOffset.ToOffset(offset);
    }
    public static async Task<DateTimeOffset?> GetNextProjectExpirationDate(this IQueryable<Team> teams, CancellationToken cancellationToken = default)
    {
        var nextDateUtc = await teams.SelectMany(t => t.Project!.Details)
         .Where(d => d.ProjectEndDate > DateTimeOffset.Now)
         .OrderBy(d => d.ProjectEndDate)
         .Select(d => d.ProjectEndDate)
         .FirstOrDefaultAsync(cancellationToken);
        return nextDateUtc;
    }
    public static List<Team> GetTeamsWithExpiredProject(this IQueryable<Team> teams) =>
      teams.Where(t => t.Project!.Details
          .Any(d => d.ProjectEndDate <= DateTimeOffset.Now))
      .ToList();

    public static DateTimeOffset GetprojectStartDate(this ProjectAssociation projet) => projet.Details.First().ProjectStartDate;
    public static DateTimeOffset GetprojectEndDate(this ProjectAssociation projet) => projet.Details.First().ProjectEndDate;
    public static DateTimeOffset GetprojectMaxEndDate(this ProjectAssociation projet) => projet.Details.Max(p => p.ProjectEndDate);
    public static string? GetprojectName(this ProjectAssociation projet) => projet.Details.Select(p => p.ProjectName).FirstOrDefault();
    public static bool IsEmpty(this ProjectAssociation projet)
       => projet.TeamManagerId == Guid.Empty && string.IsNullOrWhiteSpace(projet.TeamName) && (projet.Details == null || projet.Details.Count == 0);

    public static bool IsExpired(this ProjectAssociation projet) =>
        projet.Details.Any(d => d.ProjectEndDate <= DateTimeOffset.Now);

    public static bool HasSuspendedProject(this ProjectAssociation projet) =>
        projet.Details.Any(d => d.State == VoState.Suspended);
    public static bool HasActiveProject(this ProjectAssociation projet) =>
        projet.Details.Any(d => d.State == VoState.Active);
    public static bool DependencyExist(this ProjectAssociation Project)
    {

        if (Project == null || Project.IsEmpty())
            return false;

        bool hasDependencies = Project.HasActiveProject() || Project.HasSuspendedProject();
        if (!Project.HasActiveProject()) return hasDependencies;
        return hasDependencies;
    }
    public static Team ReviewProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new NotFoundException("Team", managerId);
        // team.MarkProjectUnderReview(projectName);
        return team;
    }
    public static Team ExpireProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new NotFoundException("Team", managerId);
        // team.RemoveExpiredProjects(projectName);
        return team;
    }
    public static Team UnassignProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new NotFoundException("Team", managerId);
        // team.UnassignProject(projectName);
        return team;
    }
    public static Team ReassignProjectAsync(Guid managerId, string projectName, DateTime newEndDate, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new NotFoundException("Team", managerId);
        // team.ReassignProject(projectName, newEndDate);
        return team;
    }
    public static Team ExtendProjectAsync(Guid managerId, string projectName, DateTime newEndDate, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new NotFoundException("Team", managerId);
        // team.ExtendProject(projectName, newEndDate);
        return team;
    }
}