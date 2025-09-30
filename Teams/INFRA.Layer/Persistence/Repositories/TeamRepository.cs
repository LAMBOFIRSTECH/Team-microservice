using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.Interfaces;
using Teams.INFRA.Layer.Persistence.EFQueries;

namespace Teams.INFRA.Layer.Persistence.Repositories;

public class TeamRepository(TeamDbContext _context) : ITeamRepository
{
    #region Get Methods
    public async Task<Team?> GetTeamByIdAsync(
        Guid teamId,
        CancellationToken cancellationToken = default
    ) => await _context.Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

    public async Task<Team?> GetTeamByNameAsync(string teamName, CancellationToken cancellationToken = default)
     => await _context.Teams.FirstOrDefaultAsync(t => t.Name.Value.Equals(teamName), cancellationToken);

    public async Task<List<Team>> GetAllTeamsAsync(CancellationToken cancellationToken = default, bool asNoTracking = false)
    {
        var query = _context.Teams.AsQueryable();
        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }
    #endregion

    #region Get by Foreign Keys
    public async Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        var teams = await _context
            .Teams!.Where(t => t.TeamManagerId.Value == managerId)
            .ToListAsync(cancellationToken);
        return teams;
    }

    public async Task<Team?> GetTeamByNameAndTeamManagerIdAsync(string teamName, Guid teamManager, CancellationToken cancellationToken = default)
    => await _context.Teams!.Where(t => t.Name.Value == teamName && t.TeamManagerId.Value.Equals(teamManager))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    => await _context.Teams.WhereMembersContain(memberId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Team>> GetTeamsByMemberAsync(Guid memberId, CancellationToken ct)
    => await _context.Teams.WhereMembersContain(memberId).ToListAsync(ct);

    public async Task<Team?> GetTeamByNameAndMemberIdAsync(Guid memberId, string teamName, CancellationToken cancellationToken)
    => await _context.Teams!.Where(t => t.Name.Value == teamName && t.MembersIds.Select(m => m.Value).Contains(memberId))
            .FirstOrDefaultAsync(cancellationToken);
    #endregion

    #region Create, Update, Delete Methods

    public async Task<Team> CreateTeamAsync(Team team, CancellationToken cancellationToken = default)
    {
        await _context.Teams.AddAsync(team, cancellationToken);
        await SaveAsync(cancellationToken);
        return team;
    }

    public async Task DeleteTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await _context
            .Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);
        _context.Teams.Remove(team!);
        await SaveAsync(cancellationToken);
    }

    public async Task UpdateTeamAsync(Team team, CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken); // voir si on doit faire un attach avant de SaveChangesAsync

    public async Task AddTeamMemberAsync(CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken);

    public async Task DeleteTeamMemberAsync(CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken);

    #endregion

    #region Project Expiry / Computation
    public async Task<List<Team>> GetTeamsWithExpiredProject(CancellationToken cancellationToken = default)
    => await _context.Teams.Where(t => t.Project!.Details.Any(d => d.ProjectEndDate <= DateTime.Now))
            .ToListAsync(cancellationToken);

    public async Task<DateTime?> GetNextProjectExpirationDate(CancellationToken cancellationToken = default)
    => await _context.Teams
           .Where(t => t.Project!.Details.Any(d => d.ProjectEndDate > DateTime.Now))
           .SelectMany(t => t.Project!.Details)
           .Where(d => d.ProjectEndDate > DateTime.Now)
           .MinAsync(d => (DateTime?)d.ProjectEndDate, cancellationToken);

    #endregion

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);

    }

}