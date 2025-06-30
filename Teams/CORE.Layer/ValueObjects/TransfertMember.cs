namespace Teams.CORE.Layer.ValueObjects;

public class AffectationStatus
{
    public bool IsTransferAllowed { get; }
    public DateTime LeaveDate { get; }

    public AffectationStatus(bool isTransferAllowed, DateTime leaveDate)
    {
        IsTransferAllowed = isTransferAllowed;
        LeaveDate = leaveDate;
    }
}

public class TransfertMember
{
    public Guid MemberTeamId { get; }
    public string SourceTeam { get; }
    public string DestinationTeam { get; }
    public AffectationStatus AffectationStatus { get; }

    public TransfertMember(
        Guid memberTeamId,
        string sourceTeam,
        string destinationTeam,
        AffectationStatus affectationStatus
    )
    {
        MemberTeamId = memberTeamId;
        SourceTeam = sourceTeam;
        DestinationTeam = destinationTeam;
        AffectationStatus = affectationStatus;
    }
}
