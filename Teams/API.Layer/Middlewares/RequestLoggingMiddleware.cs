namespace Teams.API.Layer.Middlewares;

public class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger
)
{
    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        using (
            logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId })
        )
        {
            logger.LogInformation(
                "Handling request {Method} {Path}",
                context.Request.Method,
                context.Request.Path
            );
            await next(context);
            logger.LogInformation("Finished handling request.");
        }
    }
}
