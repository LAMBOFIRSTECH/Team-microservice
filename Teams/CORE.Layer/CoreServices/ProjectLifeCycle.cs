using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Exceptions;

namespace Teams.CORE.Layer.CoreServices;

public class ProjectLifeCycle
{
    public async Task<Team> SuspendProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new NotFoundException("Team", managerId);
        team.RemoveSuspendedProjects(projectName);
        return team;
    }
}
