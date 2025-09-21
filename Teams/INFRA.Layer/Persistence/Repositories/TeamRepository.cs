using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;
using Teams.INFRA.Layer.Persistence.EFQueries;

namespace Teams.INFRA.Layer.Persistence.Repositories;

public class TeamRepository(TeamDbContext teamDbContext) : ITeamRepository
{
    public async Task<Team?> GetTeamByIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default
    ) => await teamDbContext.Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken); // AsNoTracking() pour lecture seule, sans intention de modifier

    public async Task<Team?> GetTeamByNameAsync(
        string teamName,
        CancellationToken cancellationToken = default
    ) =>
        await teamDbContext.Teams.FirstOrDefaultAsync(
            t => t.Name.Value.Equals(teamName),
            cancellationToken
        );

    public async Task<List<Team>> GetAllTeamsAsync(
        CancellationToken cancellationToken = default,
        bool asNoTracking = false
    )
    {
        var query = teamDbContext.Teams.AsQueryable();
        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Team>> GetTeamsByManagerIdAsync(
        Guid managerId,
        CancellationToken cancellationToken = default
    )
    {
        var teams = await teamDbContext
            .Teams!.Where(t => t.TeamManagerId.Value == managerId)
            .ToListAsync(cancellationToken);
        return teams;
    }

    public async Task<Team?> GetTeamByNameAndTeamManagerIdAsync(
        string teamName,
        Guid teamManager,
        CancellationToken cancellationToken = default
    ) =>
        await teamDbContext
            .Teams!.Where(t => t.Name.Value == teamName && t.TeamManagerId.Value.Equals(teamManager))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Team>> GetTeamsByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default
    )
    {
        return await teamDbContext
            .Teams.WhereMembersContain(memberId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Team>> GetTeamsByMemberAsync(
        Guid memberId,
        CancellationToken ct
    )
    {
        var teams = await teamDbContext.Teams.WhereMembersContain(memberId).ToListAsync(ct);
        return teams;
    }

    public async Task<Team?> GetTeamByNameAndMemberIdAsync(
        Guid memberId,
        string teamName,
        CancellationToken cancellationToken
    ) =>
        await teamDbContext
            .Teams!.Where(t => t.Name.Value == teamName && t.MembersIds.Select(m=> m.Value).Contains(memberId))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Team> CreateTeamAsync(
        Team team,
        CancellationToken cancellationToken = default
    )
    {
        await teamDbContext.Teams!.AddAsync(team, cancellationToken);
        await SaveAsync(cancellationToken);
        return team;
    }

    public async Task DeleteTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await teamDbContext
            .Teams.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);
        teamDbContext.Teams.Remove(team!);
        await SaveAsync(cancellationToken);
    }

    public async Task UpdateTeamAsync(Team team, CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken);

    public async Task AddTeamMemberAsync(CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken);

    public async Task DeleteTeamMemberAsync(CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken);

    public async Task SaveAsync(CancellationToken cancellationToken = default) =>
        await teamDbContext.SaveChangesAsync(cancellationToken);
}
