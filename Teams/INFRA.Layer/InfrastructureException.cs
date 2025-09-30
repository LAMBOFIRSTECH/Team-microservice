namespace Teams.INFRA.Layer;

public class InfrastructureException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }
    public string Reason { get; }

    public InfrastructureException(int statusCode, string message, string title, string reason)
        : base(message)
    {
        StatusCode = statusCode;
        Title = title;
        Reason = reason;
    }

    public static InfrastructureException InfraError(
        string message,
        string reason,
        string title = "Bad request",
        int statusCode = 400
    ) => new InfrastructureException(statusCode, message, title, reason);

    public static InfrastructureException NotFound(
        string message,
        string reason,
        string title = "Not Found",
        int statusCode = 404
    ) => new InfrastructureException(statusCode, message, title, reason);

    public static InfrastructureException BadRequest(
        string message,
        string reason,
        string title = "Bad Request",
        int statusCode = 400
    ) => new InfrastructureException(statusCode, message, title, reason);
}
