using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Teams.CORE.Entities.TeamAG;
using Teams.CORE.Entities.GeneralValueObjects;
using Teams.CORE.Entities.TeamAG.VO;

namespace Teams.INFRA.Persistence.DAL.EFMapping;

public class TeamMapping : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams", "teams");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, v => new TeamId(v))
            .HasColumnName("id");

        // Name : propriété en lecture seule -> accès via le champ privé
        builder.Property(t => t.Name)
            .HasField("_name")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion(v => v.Value, v => StringValue.Create(v))
            .HasColumnName("name")
            .HasMaxLength(150)
            .IsRequired();

        // TeamManagerId : idem, propriété en lecture seule
        builder.Property(t => t.TeamManagerId)
            .HasField("_teamManagerId")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion(v => v.Value, v => new ManagerId(v))
            .HasColumnName("team_manager_id")
            .IsRequired();

        // State : champ privé (pas une propriété) -> mapping par nom
        builder.Property<short>("State")
            .HasColumnName("state")
            .IsRequired();

        builder.Property(t => t.AverageProductivity)
            .HasConversion(v => v.Value, v => new Percentage(v))
            .HasColumnName("average_productivity")
            .IsRequired();

        builder.Property(t => t.TauxTurnover)
            .HasConversion(v => v.Value, v => new Percentage(v))
            .HasColumnName("taux_turnover")
            .IsRequired();

        builder.Property(t => t.CompositionHash)
            .HasColumnName("composition_hash");

        builder.Property(t => t.TeamCreationDate)
            .HasColumnName("team_creation_date")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(t => t.LastActivityDate)
            .HasColumnName("last_activity_date")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(t => t.TeamExpirationDate)
            .HasColumnName("team_expiration_date")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(t => t.ExtraDays)
            .HasConversion(v => v.Value, v => new ExtraDays(v))
            .HasColumnName("extra_days")
            .IsRequired();

        builder.Property(t => t.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasFilter("is_deleted = false"); // reflète uq_teams_name_actives

        // Membres : table de jointure teams.team_members
        builder.OwnsMany(t => t.TeamMembers, b =>
        {
            b.ToTable("team_members", "teams");
            b.WithOwner().HasForeignKey("team_id");
            b.Property(m => m.MemberId)
                .HasConversion(v => v!.Value, v => new MemberId(v))
                .HasColumnName("member_id");
            b.HasKey("team_id", "MemberId");
        });

        // Association projet 1:1 -> teams.project_associations
        builder.OwnsOne(t => t.ProjectAssociation, pa =>
     {
         pa.ToTable("project_associations", "teams");
         pa.WithOwner().HasForeignKey("team_id"); // PK = team_id, relation 1:1

         // ATTENTION : Guid brut dans le domaine actuel, pas un VO (ProjectId/ManagerId)
         pa.Property(p => p.ProjectId)
             .HasColumnName("project_id")
             .IsRequired();

         pa.Property(p => p.TeamManagerId)
             .HasColumnName("team_manager_id")
             .IsRequired();

         pa.Property(p => p.TeamName)
             .HasColumnName("team_name")
             .HasMaxLength(150)
             .IsRequired();

         pa.Property(p => p.State)
             .HasColumnName("state")
             .HasConversion<short>()
             .IsRequired();

         pa.Property(p => p.IsUnderReview)
             .HasColumnName("is_under_review")
             .IsRequired();
         // _details est un champ privé -> forcer l'accès par champ
         pa.Navigation(p => p.Details)
             .UsePropertyAccessMode(PropertyAccessMode.Field);

         pa.OwnsMany(p => p.Details, d =>
         {
             d.ToTable("project_association_details", "teams");
             d.WithOwner().HasForeignKey("team_id");

             // integer IDENTITY côté Postgres — pas exposé dans le domaine, shadow property
             d.Property<int>("Id")
                 .HasColumnName("id")
                 .ValueGeneratedOnAdd();
             d.HasKey("Id");

             d.Property(dd => dd.ProjectName)
                 .HasColumnName("project_name")
                 .HasMaxLength(150)
                 .IsRequired();

             d.Property(dd => dd.StartDate)
                 .HasColumnName("start_date")
                 .HasColumnType("timestamptz")
                 .IsRequired();

             d.Property(dd => dd.EndDate)
                 .HasColumnName("end_date")
                 .HasColumnType("timestamptz")
                 .IsRequired();

             d.Property(dd => dd.State)
                 .HasColumnName("state")
                 .HasConversion<short>()
                 .IsRequired();
             d.Property(dd => dd.SuspendedAt)
                   .HasColumnName("suspended_at");
         });
     });
    }
}