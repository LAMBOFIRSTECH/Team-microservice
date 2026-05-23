namespace Teams.CORE.Layer.Entities.TeamAggregate;
public interface ITeamRepository
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);
    Task CreateTeamAsync(Team team,CancellationToken cancellationToken= default);
    void UpdateTeam(Team team,CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByNameAsync(string teamName, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamWithProjectsByIdAsync(Guid id, CancellationToken cancellationToken= default);
    Task<Team?> GetTeamByNameAndMemberIdAsync(Guid memberId, string teamName, CancellationToken cancellationToken = default);
    Task<Team?> GetTeamByNameAndTeamManagerIdAsync(string teamName, Guid teamManager, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Team>> GetTeamsByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Team>> GetTeamsByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);
    Task DeleteTeamMemberAsync(CancellationToken cancellationToken = default);
    Task RedisDeleteTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default); // à dégager dans UoW et rien que
}