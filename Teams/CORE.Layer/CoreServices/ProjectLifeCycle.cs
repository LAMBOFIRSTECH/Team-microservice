using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.BusinessExceptions;
using NodaTime;

namespace Teams.CORE.Layer.CoreServices;

public class ProjectLifeCycle
{
    public async Task<Team> SuspendProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        team.RemoveSuspendedProjects(projectName);
        return team;
    }

    public async Task<Team> ReviewProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.MarkProjectUnderReview(projectName);
        return team;
    }
    public async Task<Team> ExpireProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.RemoveExpiredProjects(projectName);
        return team;
    }
    public async Task<Team> UnassignProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.UnassignProject(projectName);
        return team;
    }
    public async Task<Team> ReassignProjectAsync(Guid managerId, string projectName, DateTime newEndDate, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.ReassignProject(projectName, newEndDate);
        return team;
    }
    public async Task<Team> ExtendProjectAsync(Guid managerId, string projectName, DateTime newEndDate, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        // team.ExtendProject(projectName, newEndDate);
        return team;
    }
    public async Task<Team> AddProjectDetailAsync(Guid managerId, Detail detail, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId);
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        team.Project!.AddDetail(detail);
        return team;
    }
    public async Task<Team> RemoveProjectDetailAsync(Guid managerId, Detail detail, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId);
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        team.Project!.RemoveDetail(detail);
        return team;
    }

}
