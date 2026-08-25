using Teams.CORE.Entities.GeneralValueObjects;
namespace Teams.CORE.Entities.TeamAG.VO;
public sealed class TeamId : Identifier<TeamId>
{
    public TeamId(Guid value) : base(value) { }
}