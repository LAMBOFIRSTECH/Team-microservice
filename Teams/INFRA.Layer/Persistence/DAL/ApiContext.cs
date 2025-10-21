using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
using Teams.INFRA.Layer.Persistence.Configurations;

namespace Teams.INFRA.Layer.Persistence.DAL;

public class ApiContext : DbContext
{
    public ApiContext(DbContextOptions<ApiContext> options) : base(options) { }
    public DbSet<Team> Teams { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TeamConfiguration());
        modelBuilder.Entity<Team>().Ignore(t => t.DomainEvents);
        modelBuilder.Ignore<LocalizationDateTime>();
        base.OnModelCreating(modelBuilder);
    }
}
