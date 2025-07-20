using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities;

namespace Teams.INFRA.Layer.Persistence;

public class TeamDbContext : DbContext
{
    public TeamDbContext(DbContextOptions<TeamDbContext> options)
        : base(options) { }

    public DbSet<Team> Teams { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>().HasKey(t => t.Id);
        base.OnModelCreating(modelBuilder);
    }
}
