using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.Exceptions;
namespace Teams.CORE.Layer.CommonExtensions;

public static partial class TeamProjectExtensions
{
    public static Team RemoveProjectDetailAsync(Guid managerId, Detail detail, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId);
        if (team == null)
            throw new NotFoundException("Team", managerId);
        team.Project!.RemoveDetail(detail);
        return team;
    }
    public static Team AddProjectDetailAsync(Guid managerId, Detail detail, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId);
        if (team == null)
            throw new NotFoundException("Team", managerId);
        team.Project!.AddDetail(detail);
        return team;
    }
    public static Team SuspendProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new NotFoundException("Team", managerId);
        team.RemoveSuspendedProjects(projectName);
        return team;
    }


}