using Teams.CORE.Layer.ValueObjects;

namespace Teams.APP.Layer.Interfaces;

public interface IProjectService
{
    Task SendProjectNotification(ProjectAssociation projectAssociation);
}
