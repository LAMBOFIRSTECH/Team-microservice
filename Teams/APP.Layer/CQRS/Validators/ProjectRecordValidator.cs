using FluentValidation;
using Teams.APP.Layer.ExternalServicesDtos;

namespace Teams.APP.Layer.CQRS.Validators;

public class ProjectRecordValidator : AbstractValidator<ProjectAssociationDto>
{
    public ProjectRecordValidator()
    {
        RuleFor(x => x.TeamManagerIdDto)
            .NotEmpty()
            .WithMessage("Team manager ID cannot be empty")
            .Must(id => id != Guid.Empty)
            .WithMessage("Team manager ID must be a valid GUID");

        RuleFor(x => x.TeamNameDto).NotEmpty().WithMessage("team name can be empty");
        RuleFor(x => x.ProjectStartDateDto)
            .NotEmpty()
            .WithMessage("Project start date cannot be empty")
            .Must(date => date != default)
            .WithMessage("Project start date must be a valid date")
            .Must(date => date <= DateTime.UtcNow);
    }
}
