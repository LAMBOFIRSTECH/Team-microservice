using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Teams.CORE.Layer.Entities.TeamAggregate;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;
using Teams.CORE.Layer.Entities.GeneralValueObjects;

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

        builder.Property(t => t.State).IsRequired();
        builder.Property(t => t.TeamCreationDate)
    //  .HasConversion(
    //      v => v.ToDateTimeUtc(),
    //      v => LocalizationDateTime.FromDateTimeUtc(v)
    //  )
     .HasColumnType("datetime2")
     .IsRequired();

        builder.Property(t => t.TeamExpirationDate)
            // .HasConversion(
            //     v => v.ToDateTimeUtc(),
            //     v => LocalizationDateTime.FromDateTimeUtc(v)
            // )
            .HasColumnType("datetime2")
            .IsRequired();


        builder.OwnsMany(t => t.MembersIds, b =>
            {
                b.WithOwner().HasForeignKey("TeamId"); // FK vers Team
                b.Property(m => m.Value).HasColumnName("MemberId"); // colonne guid
                b.HasKey("Value", "TeamId"); // PK composite
                b.ToTable("TeamMembers"); // table dédiée pour EF
            });

        builder.OwnsOne(t => t.Project, pa =>
            {
                pa.Property(p => p.TeamManagerId)
                .HasConversion(v => v, v => v)
                .IsRequired();
                pa.Property(p => p.TeamName).HasMaxLength(100);
                pa.OwnsMany(p => p.Details, d =>
                {
                    d.Property(dd => dd.ProjectName).HasMaxLength(200).IsRequired();
                    d.Property(dd => dd.ProjectStartDate)
                    //    .HasConversion(
                    //         v => v.ToDateTimeUtc(),
                    //         v => LocalizationDateTime.FromDateTimeUtc(v))
                        .IsRequired();
                    d.Property(dd => dd.ProjectEndDate)
                        // .HasConversion(
                        //     v => v.ToDateTimeUtc(),
                        //     v => LocalizationDateTime.FromDateTimeUtc(v))
                        .IsRequired();
                    d.Property(dd => dd.State).IsRequired();
                    d.ToTable("ProjectDetails"); // table séparée pour Details
                    d.WithOwner().HasForeignKey("ProjectAssociationId");
                    d.Property<Guid>("Id");
                    d.HasKey("Id");
                });
            });


    }
}

