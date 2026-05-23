using FluentValidation;
namespace Teams.APP.Layer.FeatureTeam.UpdateTeamByManager;
public sealed class UpdateTeamByManagerValidator : AbstractValidator<UpdateTeamByManagerCommand>
{
    public UpdateTeamByManagerValidator()
    {
        RuleFor(x => x.TeamName)
            .NotEmpty().WithMessage("Team name cannot be empty")
            .MaximumLength(100).WithMessage("Team name cannot exceed 100 characters");
        RuleFor(x => x.OldTeamManagerId)
            .NotEmpty().WithMessage("Old team manager ID cannot be empty")
            .Must(managerId => managerId != Guid.Empty).WithMessage("Old team manager ID cannot be an empty GUID");
        RuleFor(x => x.NewTeamManagerId)
            .NotEmpty().WithMessage("New team manager ID cannot be empty")
            .Must(managerId => managerId != Guid.Empty).WithMessage("New team manager ID cannot be an empty GUID");
    }
}
