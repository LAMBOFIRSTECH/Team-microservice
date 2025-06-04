// - interface pour gérer la  persistance des données
using Teams.CORE.Layer.Entities;
namespace Teams.CORE.Layer.Interfaces;
public interface ITeamRepository
{
    Task<List<Team>> GetAllTeamsAsync();
    Task<List<Team>> GetTeamsByNameAsync(string name);
    Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId);
    Task<List<Team>> GetTeamsByUserIdAsync(Guid userId);
    Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId);
    Task<Team?> GetTeamByIdAsync(Guid teamId);
    Task<Team> CreateTeamAsync(Team team);
    Task<Team> UpdateTeamAsync(Team team);
    Task<bool> DeleteTeamAsync(Guid teamId);
}