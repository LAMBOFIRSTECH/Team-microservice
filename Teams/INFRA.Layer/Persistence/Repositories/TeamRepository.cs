using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Persistence.EFQueries;
using Teams.INFRA.Layer.Persistence.DAL;
using NodaTime;

namespace Teams.INFRA.Layer.Persistence.Repositories;

public class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(ApiContext context) : base(context) { }

    #region Get Methods
    public async Task<Team?> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default)
    => await context.Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

    public async Task<Team?> GetTeamByNameAsync(string teamName, CancellationToken cancellationToken = default)
     => await context.Teams.FirstOrDefaultAsync(t => t.Name.Value.Equals(teamName), cancellationToken);

    public async Task<List<Team>> GetAllTeamsAsync(CancellationToken cancellationToken = default, bool asNoTracking = false)
    {
        var query = context.Teams.AsQueryable();
        if (asNoTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }
    #endregion

    #region Get by Foreign Keys
    public async Task<List<Team>> GetTeamsByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        var teams = await context
            .Teams!.Where(t => t.TeamManagerId.Value == managerId)
            .ToListAsync(cancellationToken);
        return teams;
    }

    public async Task<Team?> GetTeamByNameAndTeamManagerIdAsync(string teamName, Guid teamManager, CancellationToken cancellationToken = default)
    => await context.Teams!.Where(t => t.Name.Value == teamName && t.TeamManagerId.Value.Equals(teamManager))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<Team>> GetTeamsByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    => await context.Teams.WhereMembersContain(memberId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Team>> GetTeamsByMemberAsync(Guid memberId, CancellationToken ct)
    => await context.Teams.WhereMembersContain(memberId).ToListAsync(ct);

    public async Task<Team?> GetTeamByNameAndMemberIdAsync(Guid memberId, string teamName, CancellationToken cancellationToken)
    => await context.Teams!.Where(t => t.Name.Value == teamName && t.MembersIds.Select(m => m.Value).Contains(memberId))
            .FirstOrDefaultAsync(cancellationToken);
    #endregion

    #region override herited virtual methods as possible Create, Update, Delete

    // public async Task<Team> CreateTeamAsync(Team team, CancellationToken cancellationToken = default)
    // {
    //     await context.Teams.AddAsync(team, cancellationToken);
    //     await SaveAsync(cancellationToken);
    //     return team;
    // }
    public override async Task<Team> Create(Team entity, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Création de l'équipe {entity.Name.Value}");

        return await base.Create(entity, cancellationToken);
    }


    public async Task DeleteTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await context
            .Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);
        context.Teams.Remove(team!);
        await SaveAsync(cancellationToken);
    }

    public async Task UpdateTeamAsync(Team team, CancellationToken cancellationToken = default)
    {
        context.Teams.Update(team);
        await context.SaveChangesAsync();
    }

    public async Task AddTeamMemberAsync(CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken);

    public async Task DeleteTeamMemberAsync(CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken);

    #endregion

    #region Project Expiry / Computation
    public async Task<List<Team>> GetTeamsWithExpiredProject(CancellationToken cancellationToken = default)
        => await context.Teams.Where(t => t.Project!.Details.Any(d => d.ProjectEndDate.Value.ToInstant() <= SystemClock.Instance.GetCurrentInstant()))
                .ToListAsync(cancellationToken);


    public async Task<DateTime?> GetNextProjectExpirationDate(CancellationToken cancellationToken = default)
    {
        return await context.Teams
            .Where(t => t.Project!.Details.Any(d => d.ProjectEndDate.Value.ToInstant() > SystemClock.Instance.GetCurrentInstant()))
            .SelectMany(t => t.Project!.Details)
            .Where(d => d.ProjectEndDate.Value.ToInstant() > SystemClock.Instance.GetCurrentInstant())
            .MinAsync(d => (DateTime?)d.ProjectEndDate.Value.ToDateTimeUtc(), cancellationToken);
    }
    #endregion
    public async Task SaveAsync(CancellationToken cancellationToken = default) => await context.SaveChangesAsync(cancellationToken); // dans UoW et rien que

    // private bool disposed = false;
    // protected virtual void Dispose(bool disposing)
    // {
    //     if (!this.disposed)
    //     {
    //         if (disposing)
    //         {
    //             context.Dispose();
    //         }
    //     }
    //     this.disposed = true;
    // }

    // public void Dispose()
    // {
    //     Dispose(true);
    //     GC.SuppressFinalize(this);
    // }


}