namespace Teams.APP.Layer.Interfaces;

public interface IEmployeeService
{
    Task AddTeamMemberIntoRedisCacheAsync(
        Guid memberId,
        CancellationToken cancellationToken = default
    );
    Task<bool> InsertNewTeamMemberIntoDbAsync(
        Guid memberId,
        CancellationToken cancellationToken = default
    );
    Task DeleteTeamMemberAsync(
        Guid memberId,
        string teamName,
        CancellationToken cancellationToken = default
    );
}
