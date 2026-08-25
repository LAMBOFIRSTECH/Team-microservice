using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Teams.API.Middlewares;

public class JsonValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _methodsRequiringBody = new[] { "POST", "PUT", "PATCH" };

    public JsonValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!_methodsRequiringBody.Contains(context.Request.Method.ToUpper(), StringComparer.Ordinal))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/team-management/swagger", StringComparison.Ordinal)
            || path.StartsWith("/team-management/health", StringComparison.Ordinal)
            || path.StartsWith("/team-management/hangfire", StringComparison.Ordinal)
            || path.StartsWith("/team-management/version", StringComparison.Ordinal))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        // 1. Vérification Content-Type
        if (string.IsNullOrWhiteSpace(context.Request.ContentType)
            || !context.Request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            await WriteError(context, StatusCodes.Status415UnsupportedMediaType, "Unsupported Media Type").ConfigureAwait(false);
            return;
        }

        // 2. CORRECTION : Sécurité pour le Transfer-Encoding chunked / stream vide
        // Si le ContentLength est explicitement 0, c'est vide. S'il est null, on ne bloque pas ici, on laisse le model binder d'ASP.NET Core / Newtonsoft gérer ou lever une BadHttpRequestException.
        if (context.Request.ContentLength.HasValue && context.Request.ContentLength.Value == 0)
        {
            await WriteError(context, StatusCodes.Status400BadRequest, "Request body cannot be empty").ConfigureAwait(false);
            return;
        }

        await _next(context).ConfigureAwait(false);
    }

    private async Task WriteError(HttpContext context, int statusCode, string title)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var error = new
        {
            type = $"https://tools.ietf.org/html/rfc9110#section-{statusCode}",
            title,
            status = statusCode,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonConvert.SerializeObject(error, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        }), cancellationToken: context.RequestAborted).ConfigureAwait(false);
    }
}