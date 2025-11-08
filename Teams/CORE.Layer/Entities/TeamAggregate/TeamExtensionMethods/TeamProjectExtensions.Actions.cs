using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
using Teams.CORE.Layer.Exceptions;
namespace Teams.CORE.Layer.Entities.TeamAggregate.TeamExtensionMethods;

public static partial class TeamProjectExtensions
{
    public static Team AddProjectToTeamExtension(this Team team, ProjectAssociation project)
    {
        Console.WriteLine("Dans la fonction d'ajout de projet de team projet lifecycle");
        if (project == null)
            throw new ArgumentNullException(nameof(project), "ProjectAssociation cannot be null");

        if (project.Details == null || !project.Details.Any())
            throw new InvalidOperationException("ProjectAssociation must contain at least one Detail");

        if (team.Project != null) foreach (var detail in project.Details) team.Project.AddDetail(detail); else team.AssignProject(project);
        return team;
    }
    public static Team RemoveProjectDetailAsync(Guid managerId, Detail detail, IEnumerable<Team> teams)
    {
        var team = teams.FirstOrDefault(t => t.Project != null && t.Project.TeamManagerId == managerId);
        if (team == null)
            throw new NotFoundException("Team", managerId);
        team.Project!.RemoveDetail(detail);
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