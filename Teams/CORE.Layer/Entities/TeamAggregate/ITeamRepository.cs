namespace Teams.CORE.Layer.Entities.TeamAggregate;

public interface ITeamRepository : IDisposable
{
    Task<List<Team>> GetAllTeamsAsync(CancellationToken cancellationToken = default, bool asNoTracking = false);
    Task<Team?> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByNameAsync(string teamName, CancellationToken cancellationToken = default);
    Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByNameAndMemberIdAsync(
        Guid memberId,
        string teamName,
        CancellationToken cancellationToken = default
    );
    Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByNameAndTeamManagerIdAsync(
        string teamName,
        Guid teamManager,
        CancellationToken cancellationToken = default
    );
    Task<List<Team>> GetTeamsWithExpiredProject(CancellationToken cancellationToken = default); // service du domaine
    Task<DateTime?> GetNextProjectExpirationDate(CancellationToken cancellationToken = default); // service du domaine
    Task<Team> CreateTeamAsync(Team team, CancellationToken cancellationToken = default);
    Task AddTeamMemberAsync(CancellationToken cancellationToken = default);
    Task UpdateTeamAsync(Team team, CancellationToken cancellationToken = default);
    Task DeleteTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task DeleteTeamMemberAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}
