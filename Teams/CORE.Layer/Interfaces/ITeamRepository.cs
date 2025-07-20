using Teams.CORE.Layer.Entities;

namespace Teams.CORE.Layer.Interfaces;

public interface ITeamRepository
{
    Task<List<Team>> GetAllTeamsAsync();
    Task<Team?> GetTeamByIdAsync(Guid teamId);
    Task<Team?> GetTeamByNameAsync(string teamName);
    Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId);
    Task<Team?> GetTeamByNameAndMemberIdAsync(Guid memberId, string teamName);
    Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId);
    Task<Team?> GetTeamByNameAndTeamManagerIdAsync(Guid teamManager, string teamName);
    Task<Team> CreateTeamAsync(Team team);
    Task AddTeamMemberAsync();
    Task UpdateTeamAsync(Team team);
    Task DeleteTeamAsync(Guid teamId);
    Task DeleteTeamMemberAsync();
    Task SaveAsync();
}
