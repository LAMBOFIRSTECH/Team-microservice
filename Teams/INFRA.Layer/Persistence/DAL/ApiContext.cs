using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.INFRA.Layer.Persistence.DAL.EFMapping;

namespace Teams.INFRA.Layer.Persistence.DAL;
public class ApiContext : DbContext
{
    public ApiContext(DbContextOptions<ApiContext> options) : base(options) { }
    public DbSet<Team> Teams { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TeamMapping());
        modelBuilder.Entity<Team>().Ignore(t => t.DomainEvents);
        base.OnModelCreating(modelBuilder);
    }
}
