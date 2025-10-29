using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.BusinessExceptions;
namespace Teams.CORE.Layer.CommonExtensions;

public static partial class TeamProjectExtensions
{

    public static async Task<Team> RemoveProjectDetailAsync(Guid managerId, Detail detail, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId);
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        team.Project!.RemoveDetail(detail);
        return team;
    }
    public static async Task<Team> AddProjectDetailAsync(Guid managerId, Detail detail, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId);
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        team.Project!.AddDetail(detail);
        return team;
    }
    public static async Task<Team> SuspendProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        team.RemoveSuspendedProjects(projectName);
        return team;
    }
    

}