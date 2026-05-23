using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Persistence.DAL.EFQueriesHelpers;

namespace Teams.INFRA.Layer.Persistence.DAL.Repositories;

public class TeamRepository : GenericRepository<Team>, ITeamRepository
{
    public TeamRepository(ApiContext context) : base(context) { }
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
        => await context.Teams.AnyAsync(t => t.Name.Value == name, cancellationToken);
    public async Task CreateTeamAsync(Team team, CancellationToken cancellationToken)
        => await base.AddAsync(team, cancellationToken);
    public async Task<Team?> GetTeamWithProjectsByIdAsync(Guid id, CancellationToken cancellationToken)
        => await context.Teams
            .AsNoTracking()
            .Include(t => t.Project)
                .ThenInclude(p => p.Details)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<Team?> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken)
        => await base.GetByIdAsync(teamId, cancellationToken);
    public async Task<Team?> GetTeamByNameAsync(string teamName, CancellationToken cancellationToken)
        => await base.GenericFirstOrDefaultAsync(t => t.Name.Value.Equals(teamName), cancellationToken);

    public async Task<IReadOnlyList<Team>> GetTeamsByManagerIdAsync(Guid managerId, CancellationToken cancellationToken)
        => await FindAllAsync(t => t.TeamManagerId.Value == managerId, cancellationToken);

    public async Task<Team?> GetTeamByNameAndTeamManagerIdAsync(string teamName, Guid teamManager, CancellationToken cancellationToken)
        => await base.GenericFirstOrDefaultAsync(t => t.Name.Value == teamName && t.TeamManagerId.Value == teamManager, cancellationToken);

    public async Task<IReadOnlyList<Team>> GetTeamsByMemberIdAsync(Guid memberId, CancellationToken cancellationToken)
        => await context.Teams.WhereMembersContain(memberId).ToListAsync(cancellationToken);

    public async Task<Team?> GetTeamByNameAndMemberIdAsync(Guid memberId, string teamName, CancellationToken cancellationToken)
        => await base.GenericFirstOrDefaultAsync(t => t.Name.Value == teamName && t.MembersIds.Any(m => m.Value == memberId), cancellationToken);

    public void UpdateTeam(Team team, CancellationToken cancellationToken)
        => base.Update(team, cancellationToken);

    public async Task DeleteTeamByIdAsync(Guid teamId, CancellationToken cancellationToken)
        => await base.DeleteByIdAsync(teamId, cancellationToken);

    public async Task RedisDeleteTeamByIdAsync(Guid teamId, CancellationToken cancellationToken)
        => await base.DeleteByIdAsync(teamId, cancellationToken);

    public async Task DeleteTeamMemberAsync(CancellationToken cancellationToken)
        => await SaveAsync(cancellationToken);

    // Dans UoW et rien que
    public async Task SaveAsync(CancellationToken cancellationToken) => await context.SaveChangesAsync(cancellationToken);



}