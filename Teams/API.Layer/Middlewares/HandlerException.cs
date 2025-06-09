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
}
