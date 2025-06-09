using Teams.CORE.Layer.Entities;
namespace Teams.CORE.Layer.Interfaces;
public interface ITeamRepository
{
    Task<List<Team>> GetAllTeamsAsync();
    Task<Team>? GetTeamByIdAsync(Guid teamId);
    Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId);
    Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId);
    Task<Team> CreateTeamAsync(Team team);
    Task<Team> UpdateTeamAsync(Team team);
    Task<bool> DeleteTeamAsync(Guid teamId);
}