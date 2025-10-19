using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.BusinessExceptions;

namespace Teams.CORE.Layer.CoreServices;

public class ProjectLifeCycle
{
    public void AddProjectToTeamAsync(Team team, ProjectAssociation project)
    {
        var lastDetail = project.Details.LastOrDefault();
        if (lastDetail == null)
            throw new DomainException("Project association must contain at least one project detail");

        if (team.Project is not null) team.Project!.AddDetail(lastDetail); else team.AssignProject(project);

        // Console.WriteLine($"Voici le nombre de détails présent: {project.Details.Last().ProjectName}");
    }
    public async Task<Team> SuspendProjectAsync(Guid managerId, string projectName, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId && t.Project.Details.Any(d => d.ProjectName == projectName));
        if (team == null)
            throw new DomainException("No matching team with the specified project found");
        team.RemoveSuspendedProjects(projectName);
        return team;
    }
  
}
