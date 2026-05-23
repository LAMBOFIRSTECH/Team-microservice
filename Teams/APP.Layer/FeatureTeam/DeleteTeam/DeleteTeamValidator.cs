using FluentValidation;
namespace Teams.APP.Layer.FeatureTeam.DeleteTeam;

public sealed class DeleteTeamValidator : AbstractValidator<DeleteTeamCommand>
{
    public DeleteTeamValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Team Id cannot be empty");
        RuleFor(x => x.Id).NotNull().WithMessage("Team Id cannot be null");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Team name cannot be empty");
    }
}
