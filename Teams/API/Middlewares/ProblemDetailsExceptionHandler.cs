using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Teams.CORE.Errors;
using Teams.API.Common;

namespace Teams.API.Middlewares;

public class ProblemDetailsExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/json";

        // 1. Erreurs de validation FluentValidation — cas HTTP-spécifique (400 + détail par champ)
        if (exception is ValidationException valEx)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            var errors = valEx.Errors
                .GroupBy(e => e.PropertyName ?? "body", StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray(), StringComparer.Ordinal);

            var validationJson = ProblemDetailsFactory.CreateValidationProblem(context, errors);
            await context.Response.WriteAsync(validationJson, cancellationToken).ConfigureAwait(false);
            return true;
        }

        // 2. Corps de requête techniquement invalide — cas HTTP-spécifique
        if (exception is BadHttpRequestException || exception is JsonException)
        {
            await WriteProblemResponse(context, StatusCodes.Status400BadRequest, "Invalid request body",
                new(StringComparer.Ordinal) { { "body", exception.Message } }, cancellationToken).ConfigureAwait(false);
            return true;
        }

        // 3. TOUTE exception métier/infra connue — un seul point de contrôle,
        //    plus besoin de distinguer BusinessRuleException/AppHandlerException/
        //    InfrastructureException ici : elles partagent le même contrat.
        if (exception is IHasErrorNature natureEx)
        {
            var (statusCode, title) = MapToHttp(natureEx.Nature);

            var errorJson = ProblemDetailsFactory.CreateDomainProblem(
                context,
                message: exception.Message,
                reason: natureEx.Reason,
                title: title,
                statusCode: statusCode
            );

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(errorJson, cancellationToken).ConfigureAwait(false);
            return true;
        }

        // 4. Filet de sécurité — bug système imprévu
        await WriteProblemResponse(context, StatusCodes.Status500InternalServerError, "Internal Server Error",
            new(StringComparer.Ordinal) { { "message", "An unexpected error occurred." }, { "reason", "UnhandledException" } },
            cancellationToken).ConfigureAwait(false);
        return true;
    }

    // Seule fonction de ce fichier qui "connaît" HTTP — c'est la frontière exacte
    private static (int StatusCode, string Title) MapToHttp(ErrorNature nature) => nature switch
    {
        ErrorNature.Validation          => (StatusCodes.Status400BadRequest, "Bad Request"),
        ErrorNature.NotFound            => (StatusCodes.Status404NotFound, "Resource Not Found"),
        ErrorNature.Conflict            => (StatusCodes.Status409Conflict, "Conflict Occurred"),
        ErrorNature.Unauthenticated     => (StatusCodes.Status401Unauthorized, "Authentication Required"),
        ErrorNature.Forbidden           => (StatusCodes.Status403Forbidden, "Access Denied"),
        ErrorNature.RemoteServiceFailure => (StatusCodes.Status503ServiceUnavailable, "Service Unavailable"),
        _                                => (StatusCodes.Status422UnprocessableEntity, "Application Error")
    };

    private static async Task WriteProblemResponse(HttpContext context, int statusCode, string title,
        Dictionary<string, string> errors, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = statusCode;
        var error = new
        {
            type = "https://example.com/probs/" + title.ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal),
            title,
            status = statusCode,
            errors,
            traceId = context.TraceIdentifier
        };

        var json = JsonConvert.SerializeObject(error, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        await context.Response.WriteAsync(json, cancellationToken).ConfigureAwait(false);
    }
}