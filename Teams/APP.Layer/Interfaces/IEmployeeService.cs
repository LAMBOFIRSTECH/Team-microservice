namespace Teams.APP.Layer.Interfaces;

public interface IEmployeeService
{
    Task AddTeamMemberIntoRedisCacheAsync(Guid memberId);
    Task<bool> InsertNewTeamMemberIntoDbAsync(Guid memberId);
    Task DeleteTeamMemberAsync(Guid memberId, string teamName);
}
