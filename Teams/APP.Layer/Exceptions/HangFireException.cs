namespace Teams.APP.Layer.Exceptions;

public class HangFireException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }
    public string Reason { get; }

    public HangFireException(int statusCode, string message, string title, string reason)
        : base(message)
    {
        StatusCode = statusCode;
        Title = title;
        Reason = reason;
    }

    public static HangFireException NotFound(string message, string reason) =>
        new HangFireException(404, message, "Not Found", reason);

    public static HangFireException BadRequest(string message, string reason) =>
        new HangFireException(400, message, "Bad Request", reason);

    public static HangFireException Internal(string message, string reason) =>
        new HangFireException(500, message, "Internal Error", reason);
}
