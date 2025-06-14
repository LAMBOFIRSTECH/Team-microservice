using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;

namespace Teams.INFRA.Layer.Persistence.Repositories;

public class TeamRepository(TeamDbContext teamDbContext) : ITeamRepository
{
    public async Task<Team>? GetTeamByIdAsync(Guid teamId)
    {
        return await teamDbContext.Teams.AsTracking().FirstOrDefaultAsync(t => t.Id == teamId); //AsTracking améliore la performance de la requête en évitant le suivi des modifications pour les entités récupérées, ce qui est utile si vous ne prévoyez pas de modifier ces entités dans le contexte actuel.
    }

    public async Task<List<Team>> GetAllTeamsAsync()
    {
        return await teamDbContext.Teams!.ToListAsync();
    }

    public async Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId)
    {
        var teams = await teamDbContext
            .Teams!.Where(m => m.TeamManagerId == managerId)
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

    public async Task UpdateTeamAsync(Team team)
    {
        var existingTeam = await teamDbContext.Teams!.FirstOrDefaultAsync(t => t.Id == team.Id);
        existingTeam!.Name = team.Name;
        existingTeam.MemberId = team.MemberId;
        existingTeam.TeamManagerId = team.TeamManagerId;
        existingTeam.MemberIdSerialized = JsonConvert.SerializeObject(existingTeam.MemberId);
        await teamDbContext.SaveChangesAsync();
    }

    // public async Task<bool> DeleteTeamAsync(Guid teamId)
    // {
    //     var team = await teamDbContext.Teams.FindAsync(teamId);
    //     teamDbContext.Teams.Remove(team);
    //     await teamDbContext.SaveChangesAsync();
    // }

    public async Task DeleteTeamAsync(Guid teamId)
    {
        var team = await teamDbContext.Teams!.FindAsync(teamId);
        teamDbContext.Teams.Remove(team!);
        await teamDbContext.SaveChangesAsync();
    }
    public async Task DeleteTeamMemberAsync(Guid memberId)
    {
        var team = await teamDbContext.Teams!.FindAsync(memberId);
        team!.MemberId?.Remove(memberId);
        await teamDbContext.SaveChangesAsync();
    }
}
