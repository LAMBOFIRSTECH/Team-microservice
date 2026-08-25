namespace Teams.CORE.Entities.TeamAG;

/// <summary>
/// Represents the repository interface for managing Team entities in the application.
/// This interface defines the contract for performing CRUD operations and queries related to Team entities.
/// </summary>
public interface ITeamRepository
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken =default);
    void CreateTeam(Team team);
    void UpdateTeam(Team team);
    Task<Team?> GetTeamByNameAsync(string teamName, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamWithProjectsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Team>> GetTeamsByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByNameAndMemberIdAsync(Guid memberId, string teamName, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByNameAndTeamManagerIdAsync(string teamName, Guid teamManager, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Team>> GetTeamsByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task DeleteTeamMemberAsync(CancellationToken cancellationToken = default);
    Task RedisDeleteTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default);
}