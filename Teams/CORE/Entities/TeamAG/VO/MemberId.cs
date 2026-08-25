using Teams.CORE.Entities.GeneralValueObjects;
namespace Teams.CORE.Entities.TeamAG.VO;

public sealed class MemberId : Identifier<MemberId>
{
    public MemberId(Guid value) : base(value) { }
}