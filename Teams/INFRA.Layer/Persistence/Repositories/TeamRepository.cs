using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Persistence.EFQueries;
using Teams.INFRA.Layer.Persistence.DAL;

namespace Teams.INFRA.Layer.Persistence.Repositories;

public class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(ApiContext context) : base(context) { }

    #region Get Methods

    public async Task<Team?> GetTeamByNameAsync(string teamName, CancellationToken cancellationToken = default)
     => await context.Teams.FirstOrDefaultAsync(t => t.Name.Value.Equals(teamName), cancellationToken);

    public override IQueryable<Team> GetAll(CancellationToken cancellationToken = default, params string[] includes)
    {
        base.GetAll(cancellationToken, includes);
        return context.Teams;
    }
    public async Task<Team?> GetTeamWithProjectsByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Teams
            .AsNoTracking()
            .Include(t => t.Project)
                .ThenInclude(p => p.Details)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
    public async Task<Team?> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default)
   => await context.Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

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
    public override async Task<Team> Create(Team entity, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Création de l'équipe {entity.Name.Value}");

        return await base.Create(entity, cancellationToken);
    }
    public override async void Update(Team entity)
    {
        Console.WriteLine($"Maj de l'équipe {entity.Name.Value}");
        base.Update(entity);
    }
    public override async void Delete(Team entity)
    {
        var team = await context
           .Teams.FirstOrDefaultAsync(t => t.Id == entity.Id);
        base.Delete(team!);
    }

    public async Task DeleteTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var team = await context
            .Teams.FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);
        context.Teams.Remove(team!); // pour redis
    }

    public async Task AddTeamMemberAsync(CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken);

    public async Task DeleteTeamMemberAsync(CancellationToken cancellationToken = default) =>
        await SaveAsync(cancellationToken);

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