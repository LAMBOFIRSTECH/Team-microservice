using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Teams.CORE.Layer.Entities;

namespace Teams.INFRA.Layer.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.TeamManagerId).IsRequired();
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
        builder.Property(t => t.MembersIds).IsRequired();
    }

    public void CustomTypeMapping(ModelBuilder builder)
    {
        var converter = new ValueConverter<HashSet<Guid>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v =>
                JsonSerializer.Deserialize<HashSet<Guid>>(v, (JsonSerializerOptions?)null)
                ?? new HashSet<Guid>()
        );

        var comparer = new ValueComparer<HashSet<Guid>>(
            (c1, c2) => c1!.SetEquals(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => new HashSet<Guid>(c)
        );
        builder
            .Entity<Team>()
            .Property(e => e.MembersIds)
            .HasConversion(converter)
            .Metadata.SetValueComparer(comparer);
    }
}
