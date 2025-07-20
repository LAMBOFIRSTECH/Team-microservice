namespace Teams.APP.Layer.Exceptions;

/// <summary>
/// Exception levée lors de défaillances techniques ou d'intégration (ex: service externe, infra).
/// </summary>
public class ServicesException : Exception
{
    public int Status { get; }
    public string Reason { get; }
    public string ErrorType { get; }

    public ServicesException(
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

    /// <summary>
    /// Constructeur simplifié par défaut (status = 400)
    /// </summary>
    public ServicesException(string reason, string? message, string errorType)
        : this(400, reason, message, errorType) { }

    /// <summary>
    /// Exception spécifique à l'indisponibilité d'un service externe.
    /// </summary>
    public ServicesException(string serviceName, string? detail = null)
        : this(
            503,
            $"{serviceName} Unavailable",
            $"{serviceName} is currently unavailable. {detail}",
            "External Service Error"
        ) { }
}

public static class ServicesExceptionFactory
{
    public static ServicesException Unavailable(string serviceName, string? detail = null)
    {
        return new ServicesException(serviceName, detail);
    }

    public static ServicesException Timeout(string serviceName)
    {
        return new ServicesException(
            504,
            $"{serviceName} Timeout",
            $"The request to {serviceName} timed out.",
            "External Service Timeout"
        );
    }

    public static ServicesException BadGateway(string serviceName)
    {
        return new ServicesException(
            502,
            $"{serviceName} Bad Gateway",
            $"Received an invalid response from {serviceName}.",
            "Gateway Error"
        );
    }

    public static ServicesException IntegrationFailure(string serviceName, string? detail = null)
    {
        return new ServicesException(
            500,
            $"{serviceName} Integration Failure",
            $"An unexpected error occurred while communicating with {serviceName}. {detail}",
            "Integration Error"
        );
    }
}
