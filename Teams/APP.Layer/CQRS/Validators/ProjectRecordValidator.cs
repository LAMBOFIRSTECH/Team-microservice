using FluentValidation;
using Teams.APP.Layer.ExternalServicesDtos;

namespace Teams.APP.Layer.CQRS.Validators;

public class ProjectRecordValidator : AbstractValidator<ProjectAssociatedDto>
{
    public ProjectRecordValidator()
    {
        RuleFor(x => x.TeamIdDto)
            .NotEmpty()
            .WithMessage("Team ID cannot be empty")
            .Must(id => id != Guid.Empty)
            .WithMessage("Team ID must be a valid GUID");

        RuleFor(x => x.TeamNameDto).NotEmpty().WithMessage("team name can be empty");
        RuleFor(x => x.ProjectStartDateDto)
            .NotEmpty()
            .WithMessage("Project start date cannot be empty")
            .Must(date => date != default(DateTime))
            .WithMessage("Project start date must be a valid date")
            .Must(date => date <= DateTime.UtcNow);
    }
}
