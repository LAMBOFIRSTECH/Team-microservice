using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Teams.CORE.Layer.Entities;

namespace Teams.INFRA.Layer.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.TeamManagerId).IsRequired();

        // Mapping des champs privés pour EF Core
        builder
            .Property(t => t.ProjectStartDate)
            .HasField("_projectStartDate") // le champ privé
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired(false); // nullable

        builder
            .Property(t => t.ProjectEndDate)
            .HasField("_projectEndDate")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired(false); // nullable

        builder.Property(t => t.State).IsRequired();
        builder.Property(t => t.TeamCreationDate).IsRequired();

        // Si MembersIds est sérialisé A revoir
        builder.Ignore(t => t.MembersIds);
        builder.Property(t => t.MemberIdSerialized);
    }
}
