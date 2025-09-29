namespace Teams.API.Layer.Models;

public class ValidationErrorResponse
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int Status { get; set; }
    public string? TraceId { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
}

public class ValidationError
{
    public string? Field { get; set; }
    public string? Message { get; set; }
}
