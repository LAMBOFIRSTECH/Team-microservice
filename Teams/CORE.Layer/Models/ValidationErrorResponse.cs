namespace Teams.CORE.Layer.Models;

public class ValidationErrorResponse
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public int Status { get; set; }
    public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
}

public class ValidationError
{
    public string? Field { get; set; }
    public string? Message { get; set; }
}
