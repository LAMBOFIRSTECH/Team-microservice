namespace Teams.CORE.Layer.Models;

public class Message
{
    public int Status { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
    public string? Detail { get; set; }

    public Message(int statusCode, string type, string title, string detail)
    {
        Status = statusCode;
        Title = title;
        Type = type;
        Detail = detail;
    }
}
