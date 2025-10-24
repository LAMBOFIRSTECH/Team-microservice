namespace Teams.CORE.Layer.Entities.TeamAggregate;

public interface ITeamRepository
{
    Task<Team?> GetTeamByNameAsync(string teamName, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamWithProjectsByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByNameAndMemberIdAsync(Guid memberId, string teamName, CancellationToken cancellationToken = default);
    Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByNameAndTeamManagerIdAsync(string teamName, Guid teamManager, CancellationToken cancellationToken = default);
    Task<List<Team>> GetTeamsWithExpiredProject(CancellationToken cancellationToken = default);
    Task<DateTime?> GetNextProjectExpirationDate(CancellationToken cancellationToken = default);
    Task AddTeamMemberAsync(CancellationToken cancellationToken = default);
    Task DeleteTeamMemberAsync(CancellationToken cancellationToken = default);
    Task DeleteTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}
