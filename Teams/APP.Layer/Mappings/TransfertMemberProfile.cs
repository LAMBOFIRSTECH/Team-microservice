using AutoMapper;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;
using Teams.APP.Layer.DTOs.Output;

namespace Teams.APP.Layer.Mappings;
public class TransfertMemberProfile : Profile
{
    public TransfertMemberProfile()
    {
        CreateMap<DTOs.Input.AffectationStatus,
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
                        context.Mapper.Map<CORE.Layer.Entities.TeamAggregate.TeamValueObjects.AffectationStatus>(
                            dto.AffectationStatus
                        )
                    )
            );
    }
}
