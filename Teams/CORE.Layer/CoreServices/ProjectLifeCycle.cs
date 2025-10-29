using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.BusinessExceptions;
using NodaTime;

namespace Teams.CORE.Layer.CoreServices;

// On va enlever une fois rabbitMq paramétré correctement
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
}
