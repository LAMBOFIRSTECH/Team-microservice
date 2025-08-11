using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.INFRA.Layer.Persistence.Repositories;

public class TeamRepository(TeamDbContext teamDbContext) : ITeamRepository
{
    public async Task<Team?> GetTeamByIdAsync(Guid teamId) =>
        await teamDbContext.Teams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == teamId); // AsNoTracking() pour lecture seule, sans intention de modifier

    public async Task<Team?> GetTeamByNameAsync(string teamName) =>
        await teamDbContext.Teams.FirstOrDefaultAsync(t => t.Name.Equals(teamName));

    public async Task<List<Team>> GetAllTeamsAsync() => await teamDbContext.Teams.ToListAsync();

    public async Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId)
    {
        var teams = await teamDbContext
            .Teams!.Where(m => m.TeamManagerId == managerId)
            .ToListAsync();
        return teams;
    }

    public async Task<Team?> GetTeamByNameAndTeamManagerIdAsync(
        string teamName,
        Guid teamManager
    ) =>
        await teamDbContext
            .Teams!.Where(t => t.Name == teamName && t.TeamManagerId.Equals(teamManager))
            .FirstOrDefaultAsync();

    public async Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId)
    {
        var listOfteams = await teamDbContext.Teams!.ToListAsync();
        var teams = listOfteams.Where(m => m.MembersIds.Contains(memberId)).ToList();
        return teams;
    }

    public async Task<Team?> GetTeamByNameAndMemberIdAsync(Guid memberId, string teamName) =>
        await teamDbContext
            .Teams!.Where(t => t.Name == teamName && t.MembersIds.Contains(memberId))
            .FirstOrDefaultAsync();

    public async Task<Team> CreateTeamAsync(Team team)
    {
        await teamDbContext.Teams!.AddAsync(team);
        await teamDbContext.SaveChangesAsync();
        return team;
    }

    public async Task DeleteTeamAsync(Guid teamId)
    {
        var team = await teamDbContext.Teams!.FindAsync(teamId);
        teamDbContext.Teams.Remove(team!);
        await teamDbContext.SaveChangesAsync();
    }

    public async Task UpdateTeamAsync(Team team) => await SaveAsync();

    public async Task AddTeamMemberAsync() => await SaveAsync();

    public async Task DeleteTeamMemberAsync() => await SaveAsync();

    public async Task SaveAsync() => await teamDbContext.SaveChangesAsync();
    // public async Task<bool> DeleteTeamAsync(Guid teamId)
    // {
    //     var team = await teamDbContext.Teams.FindAsync(teamId);
    //     teamDbContext.Teams.Remove(team);
    //     await teamDbContext.SaveChangesAsync();
    // }
}
