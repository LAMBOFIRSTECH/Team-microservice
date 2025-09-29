using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Teams.API.Layer.Common;

public static class ProblemDetailsFactory
{
    public static string CreateValidationProblem(
        HttpContext context,
        IDictionary<string, string[]> errors,
        string title = "One or more validation errors occurred."
    )
    {
        var problem = new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title,
            status = StatusCodes.Status400BadRequest,
            errors,
            traceId = context.TraceIdentifier
        };

        return JsonConvert.SerializeObject(problem, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
    }
    public static string CreateDomainProblem(
        HttpContext context,
        string reason,
        string message,
        int statusCode = StatusCodes.Status400BadRequest
    )
    {
        var problem = new
        {
            type = "https://example.com/probs/domain-error", // Créer une page d'erreur pour le domaine
            title = "Domain validation error",
            status = statusCode,
            errors = new Dictionary<string, string[]>()
            {
                { reason ?? "domain", new[] { message } }
            },
            traceId = context.TraceIdentifier
        };

        return JsonConvert.SerializeObject(problem, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
    }

    public static string CreateServerProblem(HttpContext context, Exception ex)
    {
        var problem = new
        {
            type = "https://example.com/probs/internal-server-error", // pour tout type d'erreurs dans le serveur
            title = "Internal Server Error",
            status = StatusCodes.Status500InternalServerError,
            message = ex.Message,
            reason = "UnhandledException",
            traceId = context.TraceIdentifier
        };

        return JsonConvert.SerializeObject(problem, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
    }
}
