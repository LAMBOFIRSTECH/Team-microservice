using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.ValueObjects;

namespace Teams.INFRA.Layer.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name)
        .HasConversion(
            v => v.Value,
            v => TeamName.Create(v)
        ).HasMaxLength(100);

        builder.Property(t => t.TeamManagerId)
            .HasConversion(
                v => v.Value,       
                v => new MemberId(v)
            ).IsRequired();
        builder
            .Property(t => t.ProjectStartDate)
            .HasField("_projectStartDate")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired(false); // nullable

        builder
            .Property(t => t.ProjectEndDate)
            .HasField("_projectEndDate")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .IsRequired(false); // nullable

        builder.Property(t => t.State).IsRequired();
        builder.Property(t => t.TeamCreationDate).IsRequired();
        builder.OwnsMany(t => t.MembersIds, b =>
            {
                b.WithOwner().HasForeignKey("TeamId"); // FK vers Team
                b.Property(m => m.Value).HasColumnName("MemberId"); // colonne guid
                b.HasKey("Value", "TeamId"); // PK composite
                b.ToTable("TeamMembers"); // table dédiée pour EF
            });
    }
}
