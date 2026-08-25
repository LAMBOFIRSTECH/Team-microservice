using Microsoft.EntityFrameworkCore;
using Teams.CORE.CoreInterfaces;
using Teams.CORE.Entities.TeamAG;
using Teams.INFRA.Persistence.DAL.EFMapping;

namespace Teams.INFRA.Persistence.DAL;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<Team> Teams { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Scanne l'assembly de la couche infra à la recherche de toutes les classes qui implémentent IEntityTypeConfiguration et appelle la méthode Configure de l'ob mappé
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        // Pour tous les Aggregats on va ignorer la persistence DomainEvents en base
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasDomainEvents).IsAssignableFrom(entityType.ClrType))
                modelBuilder.Entity(entityType.ClrType).Ignore(nameof(IHasDomainEvents.DomainEvents));
        }
        base.OnModelCreating(modelBuilder);
    }
}