using Teams.CORE.Entities.GeneralValueObjects;
namespace CORE.Entities.TeamAG.VO;

public sealed class TeamProductivityHistoryId : Identifier<TeamProductivityHistoryId>
{
    public TeamProductivityHistoryId(Guid value) : base(value) { }
}