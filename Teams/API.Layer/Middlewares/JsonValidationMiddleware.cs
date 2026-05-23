using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Teams.API.Layer.Middlewares;

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
        // Ne s’applique que sur POST/PUT/PATCH
        if (!_methodsRequiringBody.Contains(context.Request.Method.ToUpper()))
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        // Ignorer certains endpoints
        if (path.StartsWith("/team-management/swagger")
            || path.StartsWith("/team-management/health")
            || path.StartsWith("/team-management/hangfire")
            || path.StartsWith("/team-management/version"))
        {
            await _next(context);
            return;
        }

        // Vérification Content-Type
        if (string.IsNullOrWhiteSpace(context.Request.ContentType)
            || !context.Request.ContentType.Contains("application/json"))
        {
            await WriteError(context, StatusCodes.Status415UnsupportedMediaType, "Unsupported Media Type");
            return;
        }

        // Vérification corps vide
        if (context.Request.ContentLength == null || context.Request.ContentLength == 0)
        {
            await WriteError(context, StatusCodes.Status400BadRequest, "Request body cannot be empty");
            return;
        }

        await _next(context);
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
        }));
    }
}