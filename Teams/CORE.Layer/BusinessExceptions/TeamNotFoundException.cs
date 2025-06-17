namespace Teams.CORE.Layer.BusinessExceptions;

public class TeamNotFoundException : Exception
{
    public TeamNotFoundException(Guid identifier)
        : base($"Team with ID {identifier} not found.") { }

    public TeamNotFoundException()
        : base() { }

    public TeamNotFoundException(string? message)
        : base(message) { }

    public TeamNotFoundException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
