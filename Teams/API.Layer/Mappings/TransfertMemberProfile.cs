using AutoMapper;
using Teams.CORE.Layer.Entities.ValueObjects;
using Teams.INFRA.Layer.ExternalServicesDtos;

namespace Teams.API.Layer.Mappings;

public class TransfertMemberProfile : Profile
{
    public TransfertMemberProfile()
    {
        CreateMap<
            Teams.INFRA.Layer.ExternalServicesDtos.AffectationStatus,
            Teams.CORE.Layer.Entities.ValueObjects.AffectationStatus
        >()
            .ConstructUsing(dto => new Teams.CORE.Layer.Entities.ValueObjects.AffectationStatus(
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
                        context.Mapper.Map<Teams.CORE.Layer.Entities.ValueObjects.AffectationStatus>(
                            dto.AffectationStatus
                        )
                    )
            );
    }
}
