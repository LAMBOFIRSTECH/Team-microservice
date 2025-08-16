using Microsoft.EntityFrameworkCore;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Entities;

namespace Teams.INFRA.Layer.Persistence;

public class TeamDbContext : DbContext
{
    private readonly ITeamStateUnitOfWork unitOfWork;

    public TeamDbContext(DbContextOptions<TeamDbContext> options, ITeamStateUnitOfWork unitOfWork)
        : base(options)
    {
        this.unitOfWork = unitOfWork;
    }

    public DbSet<Team> Teams { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>().HasKey(t => t.Id);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var teams = ChangeTracker
            .Entries<Team>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();
        unitOfWork.RecalculateTeamStates(teams);

        return await base.SaveChangesAsync(cancellationToken);
    }
}
