using Microsoft.EntityFrameworkCore;
using Teams.CORE.Entities.TeamAG;
using Teams.INFRA.Persistence.DAL.EFMapping;

namespace Teams.INFRA.Persistence.DAL.Repositories;

public class TeamRepository(ApplicationDbContext context) : GenericRepository<Team, Guid>(context), ITeamRepository
{
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => await context.Teams.AnyAsync(t => t.Name.Value == name, cancellationToken).ConfigureAwait(false);

    public void CreateTeam(Team team) => base.Add(team);
    public void UpdateTeam(Team team) => base.Update(team);

    public async Task<Team?> GetTeamWithProjectsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Teams
            .AsNoTracking()
            .Include(t => t.ProjectAssociation)
                .ThenInclude(p => p!.Details)
            .FirstOrDefaultAsync(t => t.Id.Value == id, cancellationToken).ConfigureAwait(false);

    public async Task<Team?> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        => await base.GetByIdAsync(teamId, cancellationToken).ConfigureAwait(false);

    public async Task<Team?> GetTeamByNameAsync(string teamName, CancellationToken cancellationToken = default)
        => await base.FirstOrDefaultAsync(t => t.Name.Value.Equals(teamName), cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Team>> GetTeamsByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default)
        => await FindAllAsync(t => t.TeamManagerId.Value.Equals(managerId), cancellationToken).ConfigureAwait(false);

    public async Task<Team?> GetTeamByNameAndTeamManagerIdAsync(string teamName, Guid teamManager, CancellationToken cancellationToken = default)
        => await base.FirstOrDefaultAsync(t => t.Name.Value.Equals(teamName) && t.TeamManagerId.Value.Equals(teamManager), cancellationToken).ConfigureAwait(false);

    public async Task<IReadOnlyList<Team>> GetTeamsByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
        => await context.Teams.WhereMembersContain(memberId).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<Team?> GetTeamByNameAndMemberIdAsync(Guid memberId, string teamName, CancellationToken cancellationToken = default)
        => await base.FirstOrDefaultAsync(t => t.Name.Value.Equals(teamName) && t.TeamMembers.Any(m => m.MemberId.Value.Equals(memberId)), cancellationToken).ConfigureAwait(false);

    public Task<bool> DeleteTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default)
        => base.DeleteByIdAsync(teamId, cancellationToken);
    public Task DeleteTeamMemberAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RedisDeleteTeamByIdAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    // Pas de SaveAsync ici : la persistance passe exclusivement par IUnitOfWork.CommitAsync,
    // pour garantir l'atomicité entre repositories et permettre le dispatch correct des DomainEvents.
}