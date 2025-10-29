using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Microsoft.EntityFrameworkCore;
using NodatimePackage.Classes;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.API.Layer.DTOs;
using AutoMapper;
namespace Teams.CORE.Layer.CommonExtensions;

public static partial class TeamProjectExtensions
{
    private static readonly IMapper? _mapper;
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
    public static TeamDetailsDto BuildDto(this Team team)
    {
        var teamDto = _mapper!.Map<TeamDetailsDto>(team);
        if (team.Project == null || team.Project.Details.Count == 0)
        {
            teamDto.HasAnyProject = false;
            teamDto.ProjectNames = null;
        }
        else
        {
            var teamExpiration = team.TeamExpirationDate;
            var projetMaxEndDate = team.Project?.GetprojectMaxEndDate() ?? teamExpiration;
            DateTimeOffset maxDateUtc = projetMaxEndDate > teamExpiration ? projetMaxEndDate : teamExpiration;
            var localMaxDate = maxDateUtc.ToString("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            teamDto.TeamExpirationDate = localMaxDate;
            teamDto.HasAnyProject = true;
            teamDto.TeamManagerId = team.Project!.TeamManagerId;
            teamDto.Name = team.Project.TeamName;
            teamDto.ProjectNames = team.Project.Details.Select(d => d.ProjectName).ToList();
        }
        teamDto.State = team.MatureTeam();
        return teamDto;
    }
    public static async Task<Team> ReviewProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.MarkProjectUnderReview(projectName);
        return team;
    }
    public static async Task<Team> ExpireProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.RemoveExpiredProjects(projectName);
        return team;
    }
    public static async Task<Team> UnassignProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.UnassignProject(projectName);
        return team;
    }
    public static async Task<Team> ReassignProjectAsync(Guid managerId, string projectName, DateTime newEndDate, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.ReassignProject(projectName, newEndDate);
        return team;
    }
    public static async Task<Team> ExtendProjectAsync(Guid managerId, string projectName, DateTime newEndDate, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.ExtendProject(projectName, newEndDate);
        return team;
    }
}