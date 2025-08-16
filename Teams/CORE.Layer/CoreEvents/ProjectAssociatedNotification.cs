using MediatR;

namespace Teams.CORE.Layer.CoreEvents;

public class ProjectAssociatedNotification : INotification
{
    public string ProjectName { get; }
    public DateTime CreatedAt { get; }
    public Guid EventId { get; set; }
    public string? EventMessage { get; set; }

    public ProjectAssociatedNotification(
        string projectName,
        DateTime createdAt,
        string? eventMessage = null
    )
    {
        ProjectName = projectName;
        CreatedAt = createdAt;
        EventId = Guid.NewGuid();
        EventMessage = eventMessage;
    }
}
