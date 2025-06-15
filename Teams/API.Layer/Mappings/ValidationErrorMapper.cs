using FluentValidation.Results;
using Teams.CORE.Layer.Models;

namespace Teams.API.Layer.Mappings;

public class ValidationErrorMapper
{
    public static ValidationErrorResponse MapErrors(List<ValidationFailure> validationFailures)
    {
        return new ValidationErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Title = "One or more validation errors occurred.",
            Status = 400,
            Errors = validationFailures
                .Select(f => new ValidationError
                {
                    Field = f.PropertyName,
                    Message = f.ErrorMessage,
                })
                .ToList(),
        };
    }
}
