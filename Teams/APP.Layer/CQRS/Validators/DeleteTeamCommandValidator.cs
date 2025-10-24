using FluentValidation;
using Teams.APP.Layer.CQRS.Commands;

namespace Teams.APP.Layer.CQRS.Validators;

public class DeleteTeamCommandValidator : AbstractValidator<DeleteTeamCommand>
{
    public DeleteTeamCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Team Id cannot be empty");
        RuleFor(x => x.Id).NotNull().WithMessage("Team Id cannot be null");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Team name cannot be empty");
    }
}
