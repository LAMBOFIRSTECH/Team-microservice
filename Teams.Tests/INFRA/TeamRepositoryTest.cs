// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using FluentAssertions;
// using Microsoft.EntityFrameworkCore;
// using Teams.CORE.Layer.Entities;
// using Teams.INFRA.Layer.Persistence;
// using Teams.INFRA.Layer.Persistence.Repositories;
// using Xunit;

// namespace Teams.Tests.INFRA;

// public class TeamRepositoryTest
// {
//     private TeamDbContext CreateDbContext(string dbName)
//     {
//         var options = new DbContextOptionsBuilder<TeamDbContext>()
//             .UseInMemoryDatabase(databaseName: dbName)
//             .Options;
//         return new TeamDbContext(options);
//     }

//     private Team CreateTeam(
//         Guid? id = null,
//         string name = "Team A",
//         Guid? managerId = null,
//         List<Guid>? memberIds = null
//     )
//     {
//         return new Team
//         {
//             Id = id ?? Guid.NewGuid(),
//             Name = name,
//             TeamManagerId = managerId ?? Guid.NewGuid(),
//             MembersIds = memberIds ?? new List<Guid>(),
//         };
//     }

//     [Fact]
//     public async Task GetTeamByIdAsync_ReturnsTeam_IfExists()
//     {
//         var context = CreateDbContext("GetById");
//         var team = CreateTeam();
//         context.Teams.Add(team);
//         await context.SaveChangesAsync();

//         var repo = new TeamRepository(context);
//         var result = await repo.GetTeamByIdAsync(team.Id);

//         result.Should().NotBeNull();
//         result!.Id.Should().Be(team.Id);
//     }

//     [Fact]
//     public async Task GetTeamByNameAsync_ReturnsTeam_IfExists()
//     {
//         var context = CreateDbContext("GetByName");
//         var team = CreateTeam(name: "DevTeam");
//         context.Teams.Add(team);
//         await context.SaveChangesAsync();

//         var repo = new TeamRepository(context);
//         var result = await repo.GetTeamByNameAsync("DevTeam");

//         result.Should().NotBeNull();
//         result!.Name.Should().Be("DevTeam");
//     }

//     [Fact]
//     public async Task GetAllTeamsAsync_ReturnsAllTeams()
//     {
//         var context = CreateDbContext("GetAll");
//         context.Teams.AddRange(CreateTeam(name: "Team1"), CreateTeam(name: "Team2"));
//         await context.SaveChangesAsync();

//         var repo = new TeamRepository(context);
//         var result = await repo.GetAllTeamsAsync();

//         result.Should().HaveCount(2);
//     }

//     [Fact]
//     public async Task GetTeamsByManagerIdAsync_ReturnsCorrectTeams()
//     {
//         var context = CreateDbContext("GetByManager");
//         var managerId = Guid.NewGuid();
//         context.Teams.AddRange(
//             CreateTeam(name: "Team1", managerId: managerId),
//             CreateTeam(name: "Team2", managerId: Guid.NewGuid())
//         );
//         await context.SaveChangesAsync();

//         var repo = new TeamRepository(context);
//         var result = await repo.GetTeamsByManagerIdAsync(managerId);

//         result.Should().ContainSingle().Which.Name.Should().Be("Team1");
//     }

//     [Fact]
//     public async Task GetTeamByNameAndTeamManagerIdAsync_ReturnsCorrectTeam()
//     {
//         var context = CreateDbContext("GetByNameAndManager");
//         var managerId = Guid.NewGuid();
//         var team = CreateTeam(name: "SuperTeam", managerId: managerId);
//         context.Teams.Add(team);
//         await context.SaveChangesAsync();

//         var repo = new TeamRepository(context);
//         var result = await repo.GetTeamByNameAndTeamManagerIdAsync("SuperTeam", managerId);

//         result.Should().NotBeNull();
//         result!.Id.Should().Be(team.Id);
//     }

//     [Fact]
//     public async Task GetTeamByNameAndMemberIdAsync_ReturnsCorrectTeam()
//     {
//         var context = CreateDbContext("GetByNameAndMember");
//         var memberId = Guid.NewGuid();
//         var team = CreateTeam(name: "DevTeam", memberIds: new List<Guid> { memberId });
//         context.Teams.Add(team);
//         await context.SaveChangesAsync();

//         var repo = new TeamRepository(context);
//         var result = await repo.GetTeamByNameAndMemberIdAsync(
//             memberId,
//             "DevTeam",
//             CancellationToken.None
//         );

//         result.Should().NotBeNull();
//         result!.MembersIds.Should().Contain(memberId);
//     }

//     [Fact]
//     public async Task CreateTeamAsync_AddsTeam()
//     {
//         var context = CreateDbContext("CreateTeam");
//         var repo = new TeamRepository(context);
//         var team = CreateTeam(name: "NewTeam");

//         var result = await repo.CreateTeamAsync(team);

//         context.Teams.Count().Should().Be(1);
//         result.Name.Should().Be("NewTeam");
//     }

//     [Fact]
//     public async Task DeleteTeamAsync_RemovesTeam()
//     {
//         var context = CreateDbContext("DeleteTeam");
//         var team = CreateTeam();
//         context.Teams.Add(team);
//         await context.SaveChangesAsync();

//         var repo = new TeamRepository(context);
//         await repo.DeleteTeamAsync(team.Id);

//         context.Teams.Should().BeEmpty();
//     }

//     [Fact]
//     public async Task UpdateTeamAsync_SavesChanges()
//     {
//         var context = CreateDbContext("UpdateTeam");
//         var team = CreateTeam();
//         context.Teams.Add(team);
//         await context.SaveChangesAsync();

//         team.Name = "UpdatedName";
//         var repo = new TeamRepository(context);
//         await repo.UpdateTeamAsync(team);

//         var updated = await context.Teams.FindAsync(team.Id);
//         updated!.Name.Should().Be("UpdatedName");
//     }
// }
