using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Teams.API.Layer.Common;
using Teams.INFRA.Layer.Exceptions;

namespace Teams.API.Layer.Middlewares;
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context); // Laisse passer la requête au pipeline
        }
        catch (BadHttpRequestException ex)
        {
            await WriteProblemResponse(context, StatusCodes.Status400BadRequest,
                "Invalid request body",
                new Dictionary<string, string> { { "body", ex.Message } });
        }
        catch (JsonException ex)
        {
            await WriteProblemResponse(context, StatusCodes.Status400BadRequest,
                "Invalid request body",
                new Dictionary<string, string> { { "body", ex.Message } });
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName ?? "body")
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            await context.Response.WriteAsync(
                ProblemDetailsFactory.CreateValidationProblem(context, errors)
            );
        }
        catch (HandlerException ex)
        {
            await WriteDomainProblem(context, ex.StatusCode, ex.Title, ex.Message, ex.Reason ?? "domain_error");
        }
        catch (InfrastructureException ex)
        {
            await WriteDomainProblem(context, ex.StatusCode, ex.Title, ex.Message, ex.Reason ?? "infra_error");
        }
        catch (Exception ex)
        {
            await WriteProblemResponse(context, StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                new Dictionary<string, string> { { "message", ex.Message }, { "reason", "UnhandledException" } });
        }
    }

    private async Task WriteProblemResponse(HttpContext context, int statusCode, string title, Dictionary<string, string> errors)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var error = new
        {
            type = "https://example.com/probs/" + title.ToLower().Replace(" ", "-"),
            title,
            status = statusCode,
            errors,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonConvert.SerializeObject(error, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        }));
    }

    private async Task WriteDomainProblem(HttpContext context, int statusCode, string title, string message, string reason)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorJson = ProblemDetailsFactory.CreateDomainProblem(
            context,
            message: message,
            reason: reason,
            title: title,
            statusCode: statusCode
        );

        await context.Response.WriteAsync(errorJson);
    }
}