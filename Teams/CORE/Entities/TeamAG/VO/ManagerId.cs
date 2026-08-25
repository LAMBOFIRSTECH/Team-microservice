using Teams.CORE.Entities.GeneralValueObjects;
namespace Teams.CORE.Entities.TeamAG.VO;
public sealed class ManagerId : Identifier<ManagerId>
{
    public ManagerId(Guid value) : base(value) { }
}