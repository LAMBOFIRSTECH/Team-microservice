namespace Teams.APP.Layer.Interfaces;

public interface IBackgroundJobService
{
    Task ScheduleAddTeamMemberAsync(Guid employeeId);
    Task ScheduleProjectAssociationAsync(Guid employeeId);
}
