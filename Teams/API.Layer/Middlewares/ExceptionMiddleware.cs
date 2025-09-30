using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Teams.API.Layer.Common;
using Teams.INFRA.Layer;

namespace Teams.API.Layer.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) => _next = next;
    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (path.StartsWith("/team-management/swagger") ||
            path.StartsWith("/team-management/health") ||
            path.StartsWith("/team-management/hangfire") ||
            path.StartsWith("/team-management/version"))
        {
            await _next(context);
            return;
        }
        // Pas de Content-Type dans le header de la requete 
        if (context.Request.ContentType == null ||
            !context.Request.ContentType.Contains("application/json"))
        {
            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            context.Response.ContentType = "application/json";
            var error415 = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.16",
                title = "Unsupported Media Type",
                status = 415,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonConvert.SerializeObject(error415));
            return;
        }
        // Cas ou on rajoute Content-Type dans le header de la requete cepandant le corps est vide 
        if (context.Request.ContentLength == 0)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "Request body cannot be empty",
                status = 400,
                traceId = context.TraceIdentifier
            }));
            return;
        }

        try
        {
            await _next(context);
        }
        catch (BadHttpRequestException ex) // Corps vide / JSON mal formé
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var error = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "Invalid request body",
                status = 400,
                errors = new Dictionary<string, string>
                {
                    { "body", ex.Message }
                },
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonConvert.SerializeObject(error, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
        }
        catch (JsonException ex) // Membre inconnu ou manquant
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var error = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "Invalid request body",
                status = 400,
                errors = new Dictionary<string, string>
        {
            { "body", ex.Message }
        },
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonConvert.SerializeObject(error));
        }

        catch (ValidationException ex) // Fluenvalidation
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
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var errorJson = ProblemDetailsFactory.CreateDomainProblem(
                context,
                message: ex.Message,
                reason: ex.Reason ?? "domain_error",
                title: ex.Title,
                statusCode: ex.StatusCode
            );
            await context.Response.WriteAsync(errorJson);
        }
        catch (InfrastructureException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            var errorJson = ProblemDetailsFactory.CreateDomainProblem(
                context,
                message: ex.Message,
                reason: ex.Reason ?? "infra_error",
                title: ex.Title,
                statusCode: ex.StatusCode
            );
            await context.Response.WriteAsync(errorJson);
        }

        catch (Exception ex) // Exception technique non gérée
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var error = new
            {
                type = "https://example.com/probs/internal-server-error",
                title = "Internal Server Error",
                status = 500,
                message = ex.Message,
                reason = "UnhandledException",
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonConvert.SerializeObject(error, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));
        }
    }
}
