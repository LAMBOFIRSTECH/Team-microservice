using FluentValidation;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.APP.Layer.CQRS.Validators;

public class TransfertMemberRecordValidator : AbstractValidator<TransfertMemberDto>
{
    public TransfertMemberRecordValidator()
    {
        RuleFor(x => x.MemberTeamId)
            .NotEmpty()
            .WithMessage("Member ID cannot be empty")
            .Must(id => id != Guid.Empty)
            .WithMessage("Member ID must be a valid GUID");

        RuleFor(x => x.SourceTeam)
            .NotEmpty()
            .WithMessage("Source team can be empty")
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters.");
        ;

        RuleFor(x => x.DestinationTeam)
            .NotEmpty()
            .WithMessage("Destination team cannot be empty")
            .MaximumLength(100)
            .WithMessage("Team name cannot exceed 100 characters.");

        RuleFor(x => x.AffectationStatus.IsTransferAllowed)
            .Equal(true)
            .WithMessage("Transfer is not allowed for this member");

        RuleFor(x => x.AffectationStatus.LeaveDate)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Leave date must be in the past or present");
    }
}
