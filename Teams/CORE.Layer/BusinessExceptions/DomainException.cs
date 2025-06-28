namespace Teams.CORE.Layer.BusinessExceptions;

public class DomainException : Exception
{
    public DomainException()
        : base() { }

    public DomainException(string? message)
        : base(message) { }

    public DomainException(string? Type, string? Title, string? Detail, string? message)
        : base(message) { }
}
