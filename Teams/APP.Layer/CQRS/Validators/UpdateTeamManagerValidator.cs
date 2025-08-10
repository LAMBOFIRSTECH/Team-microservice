using FluentValidation;
using Teams.APP.Layer.CQRS.Commands;

namespace Teams.APP.Layer.CQRS.Validators;

public class UpdateTeamManagerValidator : AbstractValidator<UpdateTeamManagerCommand>
{
    public UpdateTeamManagerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Team name cannot be empty");
        RuleFor(x => x.OldTeamManagerId)
            .NotEmpty()
            .WithMessage("Old team manager ID cannot be empty");
        RuleFor(x => x.NewTeamManagerId)
            .NotEmpty()
            .WithMessage("New team manager ID cannot be empty");
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters");
        RuleFor(x => x.OldTeamManagerId)
            .Must(managerId => managerId != Guid.Empty)
            .WithMessage("Old team manager ID cannot be an empty GUID");
        RuleFor(x => x.NewTeamManagerId)
            .Must(managerId => managerId != Guid.Empty)
            .WithMessage("New team manager ID cannot be an empty GUID");
    }
}
