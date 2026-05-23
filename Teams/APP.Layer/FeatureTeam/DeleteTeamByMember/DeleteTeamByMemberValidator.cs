using FluentValidation;
namespace Teams.APP.Layer.FeatureTeam.DeleteTeamByMember;
public sealed class DeleteTeamByMemberValidator : AbstractValidator<DeleteTeamByMemberRequest>
{
    public DeleteTeamByMemberValidator()
    {
        RuleFor(x => x.TeamName)
            .NotEmpty()
            .WithMessage("Team name cannot be empty")
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters");
        RuleFor(x => x.MemberId)
            .NotEmpty()
            .WithMessage("Member ID cannot be empty")
            .NotEqual(Guid.Empty)
            .WithMessage("Member ID must be a valid non-empty GUID");
    }
}