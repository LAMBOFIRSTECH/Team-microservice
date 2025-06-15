using Teams.CORE.Layer.Entities;

namespace Teams.CORE.Layer.Interfaces;

public interface ITeamRepository
{
    Task<List<Team>> GetAllTeamsAsync();
    Task<Team>? GetTeamByIdAsync(Guid teamId);
    Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId);
    Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId);
    Task<Team> CreateTeamAsync(Team team);
    Task UpdateTeamAsync(Team team);
    Task DeleteTeamAsync(Guid teamId);
    Task DeleteTeamMemberAsync(Guid teamId);
}
