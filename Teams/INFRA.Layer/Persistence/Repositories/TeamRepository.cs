using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;
using Newtonsoft.Json;

namespace Teams.INFRA.Layer.Persistence.Repositories;

public class TeamRepository : ITeamRepository
{
    private readonly TeamDbContext teamDbContext;
    public TeamRepository(TeamDbContext teamDbContext)
    {
        this.teamDbContext = teamDbContext;
    }
    public async Task<Team>? GetTeamByIdAsync(Guid teamId)
    {
        var teams = await teamDbContext.Teams!.ToListAsync();
        return teams
        .Where(t => t.Id == teamId)
        .FirstOrDefault()!;
    }
    public async Task<List<Team>> GetAllTeamsAsync()
    {
        return await teamDbContext.Teams!.ToListAsync();
    }
    public async Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId)
    {
        var teams = await teamDbContext.Teams!
                                            .Where(m => m.TeamManagerId == managerId)
                                            .ToListAsync();
        return teams;
    }
    public async Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId)
    {
        var listOfteams = await teamDbContext.Teams!.ToListAsync();
        var teams = listOfteams.Where(m => m.MemberId.Contains(memberId)).ToList();
        return teams;
    }
    public async Task<Team> CreateTeamAsync(Team team)
    {
        team.MemberIdSerialized = JsonConvert.SerializeObject(team.MemberId);
        await teamDbContext.Teams!.AddAsync(team);
        await teamDbContext.SaveChangesAsync();
        return team;
    }

    public Task<bool> DeleteTeamAsync(Guid teamId)
    {
        throw new NotImplementedException();
    }
    public Task<Team> UpdateTeamAsync(Team team)
    {
        throw new NotImplementedException();
    }
}