using Teams.CORE.Layer.ValueObjects;

namespace Teams.APP.Layer.Interfaces;

public interface IBackgroundJobService
{
    void ScheduleAddTeamMemberAsync(Guid memberId);
    void ScheduleDeleteTeamMemberAsync(Guid memberId, string teamName);
    void ScheduleAddProjectToTeamAsync(Guid managerId, string teamName);
    // Task DisAffectedProjectToTeam();
}
