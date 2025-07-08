using Teams.APP.Layer.ExternalServicesDtos;
using Teams.CORE.Layer.ValueObjects;

namespace Teams.API.Layer.Mappings;

public class TeamTransfertMapper
{
    public static class TransfertMemberMapper
    {
        public static TransfertMember ToDomain(TransfertMemberDto dto)
        {
            return new TransfertMember(
                dto.MemberTeamId,
                dto.SourceTeam,
                dto.DestinationTeam,
                new Teams.CORE.Layer.ValueObjects.AffectationStatus(
                    dto.AffectationStatus.IsTransferAllowed,
                    dto.AffectationStatus.LeaveDate
                )
            );
        }
    }
}
