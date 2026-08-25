using Teams.CORE.Entities.GeneralValueObjects;
namespace CORE.Entities.TeamAG.VO;

public sealed class TeamMembersHistoryId : Identifier<TeamMembersHistoryId>
{
    public TeamMembersHistoryId(Guid value) : base(value) { }
}