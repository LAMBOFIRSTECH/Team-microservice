namespace Teams.APP.Layer.Interfaces;

public interface IEmployeeService
{
    Task AddTeamMemberIntoRedisCacheAsync(Guid memberId);
    Task InsertNewTeamMemberIntoDbAsync(Guid memberId);
    Task DeleteTeamMemberAsync(Guid memberId, string teamName);
}
