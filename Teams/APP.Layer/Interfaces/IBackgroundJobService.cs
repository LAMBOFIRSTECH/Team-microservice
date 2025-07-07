namespace Teams.APP.Layer.Interfaces;

public interface IBackgroundJobService
{
    void ScheduleAddTeamMemberAsync(Guid memberId);
    void ScheduleDeleteTeamMemberAsync(Guid memberId, string teamName);
    void ScheduleProjectAssociationAsync();
}
