namespace Teams.CORE.Layer.Exceptions;

public abstract class DomainException : Exception
{
    public int StatusCode { get; }
    public string Reason { get; }

    protected DomainException(string message, int statusCode, string reason)
        : base(message)
    {
        StatusCode = statusCode;
        Reason = reason;
    }
}
