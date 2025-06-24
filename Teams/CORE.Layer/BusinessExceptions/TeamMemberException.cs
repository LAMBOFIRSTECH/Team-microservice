namespace Teams.CORE.Layer.BusinessExceptions;

public class TeamMemberException : Exception
{
    public TeamMemberException(Guid identifier)
        : base($"Team with ID {identifier} not found.") { }

    public TeamMemberException()
        : base() { }

    public TeamMemberException(string? message)
        : base(message) { }
}
