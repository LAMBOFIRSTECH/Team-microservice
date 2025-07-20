using Teams.CORE.Layer.ValueObjects;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.API.Layer.Mappings;

public class TeamTransfertMapper
{
    public static class TeamTransfertMember
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
