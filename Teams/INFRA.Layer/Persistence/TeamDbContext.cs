using Microsoft.EntityFrameworkCore;
using Teams.CORE.Layer.Entities;
namespace Teams.INFRA.Layer.Persistence;
public class TeamDbContext : DbContext
{
    public TeamDbContext(DbContextOptions<TeamDbContext> options) : base(options) { }

    public DbSet<Team>? Teams { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>()
            .HasKey(t => t.Id);
        base.OnModelCreating(modelBuilder);

        // modelBuilder.Entity<Team>()
        //     .Ignore(t => t.MemberId); // Ignore MemberId for now, as it's not a direct property in the database
        
    }
}

