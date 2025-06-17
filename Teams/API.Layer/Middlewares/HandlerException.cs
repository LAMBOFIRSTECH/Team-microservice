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

    public static HandlerException NotFound(string message, string reason) =>
        new HandlerException(404, message, "Not Found", reason);

    public static HandlerException BadRequest(string message, string reason) =>
        new HandlerException(400, message, "Bad Request", reason);
}
