namespace Teams.CORE.Layer.BusinessExceptions;

public class DomainException : Exception
{
    public int Status { get; }
    public string Reason { get; }
    public string ErrorType { get; }

    public DomainException(
        int status,
        string reason,
        string? message,
        string errorType,
        Exception? innerException = null
    )
        : base(message, innerException)
    {
        Status = status;
        Reason = reason;
        ErrorType = errorType;
    }

    // Constructeur simplifié, par défaut 400 Bad Request
    public DomainException(string reason, string? message, string errorType)
        : this(400, reason, message, errorType) { }

    // Constructeur simple avec message uniquement
    public DomainException(string? message)
        : base(message)
    {
        Status = 400;
        Reason = "Domain Error";
        ErrorType = "Business Rule Violation";
    }
}

public static class DomainExceptionFactory
{
    public static DomainException NotFound(string entityType, object identifier) =>
        new DomainException(
            404,
            $"{entityType} Not Found",
            $"No {entityType.ToLower()} found with identifier '{identifier}'.",
            "Resource Not Found"
        );

    public static DomainException BusinessRule(string rule, string? detail = null) =>
        new DomainException(
            400,
            $"Business rule violated: {rule}",
            detail,
            "Business Rule Violation"
        );

    public static DomainException Conflict(string entityType, string reason) =>
        new DomainException(409, $"Conflict on {entityType}", reason, "Conflict Violation");

    public static DomainException Unauthorized(string reason) =>
        new DomainException(401, "Unauthorized Access", reason, "Authorization Error");
}
