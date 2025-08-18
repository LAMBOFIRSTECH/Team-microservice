namespace Teams.APP.Layer.Interfaces;

public interface IProjectService
{
    Task ManageTeamProjectAsync(Guid managerId, string teamName);
}
