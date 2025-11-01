namespace Teams.APP.Layer.Interfaces;

public interface IProjectService
{
    Task SuspendProjectAsync(string message);
    Task ProjectAssociationDataAsync(string message);
}
