namespace Teams.API.Layer.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(ms => ms.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors
                        .Select(e => NormalizeErrorMessage(e.ErrorMessage, kvp.Key))
                        .ToArray()
                );

            var json = ProblemDetailsFactory.CreateValidationProblem(context.HttpContext, errors);

            context.Result = new ContentResult
            {
                Content = json,
                StatusCode = StatusCodes.Status400BadRequest,
                ContentType = "application/json"
            };
        }
    }
    /// <summary>
    /// Manage error messages on json body structure
    /// </summary>
    /// <item><description>Sent back error message when request body contains invalid property.</description></item>
    /// <item><description>Sent back error message when json body structure is malformed.</description></item>
    /// <item><description>Sent back error message when json body structure lack some requiring field.</description></item>
    /// <param name="rawMessage"></param>
    /// <param name="fieldname"></param>
    /// <returns></returns>
    private string NormalizeErrorMessage(string rawMessage, string fieldname)
    {
        if (rawMessage.Contains("Could not find member"))
            return $"Invalid property in request body [[{fieldname}]]";

        if (rawMessage.Contains("Unexpected end when deserializing"))
            return "Malformed JSON structure";

        if (rawMessage.Contains("required"))
            return "Missing required field";
        return rawMessage;
    }
}
