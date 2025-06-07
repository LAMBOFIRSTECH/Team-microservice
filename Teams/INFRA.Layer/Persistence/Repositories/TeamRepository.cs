using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.INFRA.Layer.Persistence.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly TeamDbContext teamDbContext;
    public TeamRepository(TeamDbContext teamDbContext)
    {
        this.teamDbContext = teamDbContext;
    }
    public async Task<List<Team>> GetAllTeams()
    {
        return await teamDbContext.Teams.ToListAsync();
    }
    public async Task<Team>? GetTeamById(Guid teamId)
    {
        var teams = await GetAllTeams();
        return teams.Where(t => t.Id == teamId).First();
    }

    public Task<Team> CreateTeamAsync(Team team)
    {
        throw new NotImplementedException();
    }
    public Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteTeamAsync(Guid teamId)
    {
        throw new NotImplementedException();
    }


    public Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Team>> GetTeamsByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<List<Team>> GetTeamsByUserIdAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<Team> UpdateTeamAsync(Team team)
    {
        throw new NotImplementedException();
    }
}