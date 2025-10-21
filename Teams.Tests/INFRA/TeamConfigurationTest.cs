// using System;
// using Microsoft.EntityFrameworkCore;
// using Teams.CORE.Layer.Entities;
// using Teams.INFRA.Layer.Persistence.Configurations;
// using Xunit;

// namespace Teams.Tests.INFRA
// {
//     public class TeamConfigurationTest
//     {
//         private class TestDbContext : DbContext
//         {
//             public TestDbContext(DbContextOptions options)
//                 : base(options) { }

//             public DbSet<Team> Teams => Set<Team>();

//             protected override void OnModelCreating(ModelBuilder modelBuilder)
//             {
//                 // Appliquer la configuration de Team
//                 var teamConfig = new TeamConfiguration();
//                 teamConfig.Configure(modelBuilder.Entity<Team>());
//                 teamConfig.CustomTypeMapping(modelBuilder);
//             }
//         }

//         [Fact]
//         public void TeamConfiguration_ShouldConfigureTeamEntityProperly()
//         {
//             // Arrange
//             var options = new DbContextOptionsBuilder<TestDbContext>()
//                 .UseInMemoryDatabase(databaseName: "TestDb")
//                 .Options;

//             using var context = new TestDbContext(options);

//             // Act
//             var model = context.Model.FindEntityType(typeof(Team));

//             // Assert
//             Assert.NotNull(model);
//             Assert.Equal("Id", model.FindPrimaryKey()?.Properties[0].Name);

//             Assert.False(model.FindProperty("Name").IsNullable);
//             Assert.False(model.FindProperty("TeamManagerId").IsNullable);
//             Assert.False(model.FindProperty("State").IsNullable);
//             Assert.False(model.FindProperty("TeamCreationDate")!.IsNullable);
//             Assert.False(model.FindProperty("MembersIds").IsNullable);

//             // Propriétés nullable
//             Assert.True(model.FindProperty("ProjectStartDate").IsNullable);
//             Assert.True(model.FindProperty("ProjectEndDate").IsNullable);
//         }
//     }
// }
