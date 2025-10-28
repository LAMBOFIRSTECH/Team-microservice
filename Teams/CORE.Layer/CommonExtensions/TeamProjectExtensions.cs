using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Microsoft.EntityFrameworkCore;
using NodatimePackage.Classes;
using Teams.APP.Layer.Helpers;
namespace Teams.CORE.Layer.CommonExtensions;

public static class TeamProjectExtensions
{
   
    public static DateTimeOffset ConvertDatetimeIntoDateTimeOffset(this string timeZoneId, DateTimeOffset utcDateTimeOffset)
    {
        // Récupère le décalage exact du fuseau à la date donnée
        var (offset, _) = TimeOperations.GetTimeZoneOffsetWithDaylight(timeZoneId, utcDateTimeOffset.UtcDateTime);
        return utcDateTimeOffset.ToOffset(offset);
    }
    public static List<Team> GetTeamsWithExpiredProject(this IQueryable<Team> teams) =>
        teams.Where(t => t.Project!.Details
            .Any(d => d.ProjectEndDate <= DateTimeOffset.Now))
        .ToList();

    public static async Task<DateTimeOffset?> GetNextProjectExpirationDate(this IQueryable<Team> teams, CancellationToken cancellationToken = default)
    {
        var nextDateUtc = await teams.SelectMany(t => t.Project!.Details)
         .Where(d => d.ProjectEndDate > DateTimeOffset.Now)
         .OrderBy(d => d.ProjectEndDate)
         .Select(d => d.ProjectEndDate)
         .FirstOrDefaultAsync(cancellationToken);

        return nextDateUtc;
    }

    public static bool IsEmpty(this ProjectAssociation projet) =>
        projet.TeamManagerId == Guid.Empty &&
        string.IsNullOrWhiteSpace(projet.TeamName) &&
        (projet.Details == null || projet.Details.Count == 0);

    public static bool IsExpired(this ProjectAssociation projet) =>
        projet.Details.Any(d => d.ProjectEndDate <= DateTimeOffset.Now);

    public static bool HasSuspendedProject(this ProjectAssociation projet) =>
        projet.Details.Any(d => d.State == VoState.Suspended);
    public static bool HasActiveProject(this ProjectAssociation projet) =>
        projet.Details.Any(d => d.State == VoState.Active);

    public static DateTimeOffset GetprojectStartDate(this ProjectAssociation projet) => projet.Details.First().ProjectStartDate;
    public static DateTimeOffset GetprojectEndDate(this ProjectAssociation projet) => projet.Details.First().ProjectEndDate;
    public static DateTimeOffset GetprojectMaxEndDate(this ProjectAssociation projet) => projet.Details.Max(p => p.ProjectEndDate);
    public static bool DependencyExist(this ProjectAssociation Project)
    {

        if (Project == null || Project.IsEmpty())
            return false;

        bool hasDependencies = Project.HasActiveProject() || Project.HasSuspendedProject();
        if (!Project.HasActiveProject()) return hasDependencies;
        return hasDependencies;
    }
}
