using FluentValidation;
using Teams.APP.Layer.CQRS.Commands;
namespace Teams.APP.Layer.CQRS.Validators;
public class TeamValidator : AbstractValidator<CreateTeamCommand>
{
    public TeamValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Team name cannot be empty");
    }
}
