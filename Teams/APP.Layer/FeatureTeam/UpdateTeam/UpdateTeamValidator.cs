using FluentValidation;
namespace Teams.APP.Layer.FeatureTeam.UpdateTeam;
public sealed class UpdateTeamValidator : AbstractValidator<UpdateTeamCommand>
{
    public UpdateTeamValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Team name cannot be empty")
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters");

        RuleFor(x => x.TeamManagerId)
            .NotEmpty()
            .WithMessage("Team manager ID cannot be empty")
            .GreaterThanOrEqualTo(Guid.Empty)
            .WithMessage("Team manager ID must be a valid GUID");

        RuleFor(x => x.MembersIds)
            .NotEmpty()
            .WithMessage("Team members list cannot be empty")
            .Must(members => members.Count() >= 2)
            .WithMessage("A team must have at least 2 members.")
            .Must(members => members.All(id => id != Guid.Empty))
            .WithMessage("All team member IDs must be valid non-empty GUIDs");
    }
}
