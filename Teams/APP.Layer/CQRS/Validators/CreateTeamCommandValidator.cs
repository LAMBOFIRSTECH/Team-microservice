using FluentValidation;
using Teams.APP.Layer.CQRS.Commands;

namespace Teams.APP.Layer.CQRS.Validators;
public class CreateTeamCommandValidator : AbstractValidator<CreateTeamCommand>
{
    public CreateTeamCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Team name cannot be empty");
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters");
        RuleFor(x => x.TeamManagerId)
            .Must(id => id != Guid.Empty)
            .WithMessage("Team manager ID cannot be empty or an empty GUID");

        RuleFor(x => x.MembersIds)
            .NotEmpty()
            .WithMessage("Team member cannot be empty")
            .Must(members => members.All(id => id != Guid.Empty))
            .WithMessage("All team member IDs must be valid (non-empty GUIDs)");
    }
}
