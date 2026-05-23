namespace Teams.CORE.Layer.Exceptions;

/// <summary>
/// Exception levée lorsqu'un service externe retourne un contrat invalide
/// ou un comportement inattendu (Anti-Corruption Layer).
/// </summary>
public class ExternalServiceException : Exception
{
    public string? ExternalServiceName { get; }
    public string? Operation { get; }
    public string? ErrorCode { get; }

    public ExternalServiceException()
    {
    }

    public ExternalServiceException(string message)
        : base(message)
    {
    }

    public ExternalServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ExternalServiceException(
        string message,
        string externalServiceName,
        string operation)
        : base(message)
    {
        ExternalServiceName = externalServiceName;
        Operation = operation;
    }

    public ExternalServiceException(
        string message,
        string externalServiceName,
        string operation,
        string errorCode)
        : base(message)
    {
        ExternalServiceName = externalServiceName;
        Operation = operation;
        ErrorCode = errorCode;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, " +
               $"Service: {ExternalServiceName}, " +
               $"Operation: {Operation}, " +
               $"ErrorCode: {ErrorCode}";
    }
}