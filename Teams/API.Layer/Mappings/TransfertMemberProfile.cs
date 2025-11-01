using AutoMapper;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.API.Layer.Mappings;

public class TransfertMemberProfile : Profile
{
    public TransfertMemberProfile()
    {
        CreateMap<INFRA.Layer.ExternalServicesDtos.AffectationStatus,
            CORE.Layer.Entities.TeamAggregate.TeamValueObjects.AffectationStatus
        >()
            .ConstructUsing(dto => new CORE.Layer.Entities.TeamAggregate.TeamValueObjects.AffectationStatus(
                dto.IsTransferAllowed,
                dto.ContratType,
                dto.LeaveDate
            ));

        CreateMap<TransfertMemberDto, TransfertMember>()
            .ConstructUsing(
                (dto, context) =>
                    new TransfertMember(
                        dto.MemberTeamId,
                        dto.SourceTeam,
                        dto.DestinationTeam,
                        context.Mapper.Map<Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects.AffectationStatus>(
                            dto.AffectationStatus
                        )
                    )
            );
    }
}
