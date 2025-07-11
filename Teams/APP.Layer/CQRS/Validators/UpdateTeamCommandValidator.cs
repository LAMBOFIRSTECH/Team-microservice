using FluentValidation;
using Teams.APP.Layer.CQRS.Commands;

namespace Teams.APP.Layer.CQRS.Validators;

public class UpdateTeamCommandValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Team name cannot be empty");
        RuleFor(x => x.TeamManagerId).NotEmpty().WithMessage("Team manager ID cannot be empty");
        RuleFor(x => x.Name)
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters");
        RuleFor(x => x.TeamManagerId)
            .Must(managerId => managerId != Guid.Empty)
            .WithMessage("Team manager ID cannot be an empty GUID");
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("Team member cannot be an empty list")
            .Must(members => members.All(id => id != Guid.Empty))
            .WithMessage("All team member IDs must be valid (non-empty GUIDs)")
            .Must(members => members.Count >= 2)
            .WithMessage("A team must have at least 2 members.");
    }
}
