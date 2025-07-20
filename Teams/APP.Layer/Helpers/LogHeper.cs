namespace Teams.APP.Layer.Helpers;

public static class LogHelper
{
    public static void BusinessRuleViolated(
        ILogger logger,
        string rule,
        Guid memberId,
        string? detail = null
    ) =>
        logger.LogWarning(
            "Business rule violated: {Rule}. Member: {MemberId}. {Detail}",
            rule,
            memberId,
            detail
        );

    public static void CriticalFailure(
        ILogger logger,
        string context,
        string message,
        Exception? ex = null
    ) => logger.LogCritical(ex, "Critical failure in {Context}: {Message}", context, message);

    public static void Info(ILogger logger, string message) =>
        logger.LogInformation("Info: {Message}", message);

    // rajouter un autre logWarning
}
