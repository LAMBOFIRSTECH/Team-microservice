using FluentValidation;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.APP.Layer.CQRS.Validators;

public class ProjectRecordValidator : AbstractValidator<ProjectAssociationDto>
{
    public ProjectRecordValidator()
    {
        RuleFor(x => x.TeamManagerId)
            .NotEmpty()
            .WithMessage("Team manager ID cannot be empty.")
            .Must(id => id != Guid.Empty)
            .WithMessage("Team manager ID must be a valid GUID.");

        RuleFor(x => x.TeamName)
            .NotEmpty()
            .WithMessage("Team name can be empty.")
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters.");

        RuleForEach(x => x.Details).NotNull().WithMessage("project details can be empty.");

        RuleForEach(x => x.Details)
            .ChildRules(detail =>
            {
                detail
                    .RuleFor(x => x.ProjectName)
                    .NotEmpty()
                    .WithMessage("Project name can be empty.")
                    .MaximumLength(200)
                    .WithMessage("Project name cannot exceed 200 characters.");
            });

        RuleForEach(x => x.Details)
            .ChildRules(detail =>
            {
                detail
                    .RuleFor(x => x.ProjectStartDate)
                    .NotEmpty()
                    .WithMessage("Project start date cannot be empty.")
                    .Must(date => date != DateTime.MinValue)
                    .WithMessage("Project start date must be a valid date.")
                    .Must(date => date >= DateTime.UtcNow)
                    .WithMessage("Project start date cannot be in the past.");
            });
        RuleForEach(x => x.Details)
            .ChildRules(detail =>
            {
                detail
                    .RuleFor(x => x.ProjectEndDate)
                    .NotEmpty()
                    .WithMessage("Project end date cannot be empty.")
                    .Must(date => date != DateTime.MinValue)
                    .WithMessage("Project end date must be a valid date.")
                    .Must(date => date >= DateTime.UtcNow)
                    .WithMessage("Project end date cannot be in the past.")
                    .Must((dto, endDate) => endDate > dto.ProjectStartDate)
                    .WithMessage("Project end date must be after the project start date.");
            });
        RuleForEach(x => x.Details)
            .ChildRules(detail =>
            {
                detail
                    .RuleFor(x => x.ProjectState)
                    .NotNull()
                    .WithMessage("Project state object cannot be null.");
            });

        RuleForEach(x => x.Details)
            .ChildRules(detail =>
            {
                detail
                    .RuleFor(x => x.ProjectState.State)
                    .IsInEnum()
                    .WithMessage("Project state must be a valid enum value.");
            });
    }
}
