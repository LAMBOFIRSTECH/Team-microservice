using Microsoft.EntityFrameworkCore;
using Teams.APP.Layer.Interfaces;
using Teams.CORE.Layer.Entities;
using Teams.INFRA.Layer.Dispatchers;
using Teams.INFRA.Layer.Persistence.Configurations;

namespace Teams.INFRA.Layer.Persistence;

public class TeamDbContext : DbContext
{
    private readonly ITeamStateUnitOfWork unitOfWork;
    private readonly IDomainEventDispatcher _dispatcher;

    public TeamDbContext(
        DbContextOptions<TeamDbContext> options,
        ITeamStateUnitOfWork unitOfWork,
        IDomainEventDispatcher dispatcher
    )
        : base(options)
    {
        this.unitOfWork = unitOfWork;
        _dispatcher = dispatcher;
    }

    public DbSet<Team> Teams { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TeamConfiguration());
        modelBuilder.Entity<Team>().Ignore(t => t.DomainEvents);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var teams = ChangeTracker
            .Entries<Team>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .ToList();
        unitOfWork.RecalculateTeamStates(teams);
        var entitiesWithEvents = ChangeTracker
            .Entries()
            .Where(e => e.Entity is Team t && t.DomainEvents.Count > 0)
            .Select(e => (Team)e.Entity)
            .ToList();

        var result = await base.SaveChangesAsync(ct);
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();
            await _dispatcher.DispatchAsync(events, ct);
        }

        return result;
    }
}
