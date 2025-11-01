namespace Teams.API.Layer.Middlewares;

public class HandlerException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }
    public string Reason { get; }

    public HandlerException(int statusCode, string message, string title, string reason)
        : base(message)
    {
        StatusCode = statusCode;
        Title = title;
        Reason = reason;
    }

    public static HandlerException DomainError(
        string message,
        string reason,
        string title = "Domain validation error",
        int statusCode = 400
    ) => new HandlerException(statusCode, message, title, reason);

    public static HandlerException NotFound(
        string message,
        string reason,
        string title = "Not Found",
        int statusCode = 404
    ) => new HandlerException(statusCode, message, title, reason);

    public static HandlerException BadRequest(
        string message,
        string reason,
        string title = "Bad Request",
        int statusCode = 400
    ) => new HandlerException(statusCode, message, title, reason);

    public static HandlerException TechnicalError(
     string message,
     string reason,
     string title = "Technical error",
     int statusCode = 500
 ) => new HandlerException(statusCode, message, title, reason);
}
