namespace Teams.APP.Layer.Helpers;

public static class LogHelper
{
    public static void BusinessRuleViolated(
        string rule = null!,
        Guid id = default,
        string? detail = null,
        ILogger logger = null!
    ) => logger.LogWarning(rule, id, detail, "Business rule violated: {Rule}. Id: {id}. {Detail}");

    public static void Info(string message, ILogger logger = null!) =>
        logger.LogInformation("Info: {Message}", message);

    public static void Warning(string message, ILogger logger = null!) =>
        logger.LogWarning("Warning: {Message}", message);

    public static void Error(string message, ILogger logger = null!) =>
        logger.LogError("Error: {Message}", message);

    public static void CriticalFailure(
        ILogger logger,
        string context,
        string message,
        Exception? ex = null
    ) => logger.LogCritical(ex, "Critical failure in {Context}: [ {Message} ]", context, message);

    public static void BusinessRuleFailure(
        ILogger logger,
        string context,
        string message,
        Exception? ex = null
    ) => logger.LogError(ex, "Rule Violated in {Context} => [{Message}]", context, message);
}

// 🚫 (interdiction)

// ⚠️ (avertissement)

// ❌ (erreur)

// 🛑 (arrêt)
