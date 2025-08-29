// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Teams.CORE.Layer.Entities;
// using Teams.INFRA.Layer.Persistence;
// using Teams.INFRA.Layer.Persistence.Repositories;
// using Xunit;

// namespace Teams.Tests.INFRA;

// public class TeamRepositoryTest
// {
//     private TeamDbContext GetDbContext()
//     {
//         var options = new DbContextOptionsBuilder<TeamDbContext>()
//             .UseInMemoryDatabase(Guid.NewGuid().ToString())
//             .Options;
//         return new TeamDbContext(options);
//     }

//     private Team CreateSampleTeam(
//         Guid? managerId = null,
//         string name = "Alpha",
//         List<Guid>? members = null
//     )
//     {
//         return new Team
//         {
//             Id = Guid.NewGuid(),
//             Name = name,
//             TeamManagerId = managerId ?? Guid.NewGuid(),
//             MembersIds = members ?? new List<Guid> { Guid.NewGuid() },
//         };
//     }

//     [Fact]
//     public async Task CreateTeamAsync_ShouldAddTeam()
//     {
//         var db = GetDbContext();
//         var repo = new TeamRepository(db);
//         var team = CreateSampleTeam();

//         var result = await repo.CreateTeamAsync(team);

//         Assert.Equal(team.Id, result.Id);
//         Assert.Single(db.Teams);
//     }

//     [Fact]
//     public async Task GetTeamByIdAsync_ShouldReturnTeam()
//     {
//         var db = GetDbContext();
//         var team = CreateSampleTeam();
//         db.Teams.Add(team);
//         db.SaveChanges();
//         var repo = new TeamRepository(db);

//         var result = await repo.GetTeamByIdAsync(team.Id);

//         Assert.NotNull(result);
//         Assert.Equal(team.Id, result!.Id);
//     }

//     [Fact]
//     public async Task GetTeamByNameAsync_ShouldReturnTeam()
//     {
//         var db = GetDbContext();
//         var team = CreateSampleTeam(name: "Bravo");
//         db.Teams.Add(team);
//         db.SaveChanges();
//         var repo = new TeamRepository(db);

//         var result = await repo.GetTeamByNameAsync("Bravo");

//         Assert.NotNull(result);
//         Assert.Equal("Bravo", result!.Name);
//     }

//     [Fact]
//     public async Task GetAllTeamsAsync_ShouldReturnAllTeams()
//     {
//         var db = GetDbContext();
//         db.Teams.Add(CreateSampleTeam(name: "A"));
//         db.Teams.Add(CreateSampleTeam(name: "B"));
//         db.SaveChanges();
//         var repo = new TeamRepository(db);

//         var result = await repo.GetAllTeamsAsync();

//         Assert.Equal(2, result.Count);
//     }

//     [Fact]
//     public async Task GetTeamsByManagerIdAsync_ShouldReturnTeams()
//     {
//         var db = GetDbContext();
//         var managerId = Guid.NewGuid();
//         db.Teams.Add(CreateSampleTeam(managerId: managerId));
//         db.Teams.Add(CreateSampleTeam(managerId: managerId));
//         db.Teams.Add(CreateSampleTeam(managerId: Guid.NewGuid()));
//         db.SaveChanges();
//         var repo = new TeamRepository(db);

//         var result = await repo.GetTeamsByManagerIdAsync(managerId);

//         Assert.Equal(2, result.Count);
//         Assert.All(result, t => Assert.Equal(managerId, t.TeamManagerId));
//     }

//     [Fact]
//     public async Task GetTeamByNameAndTeamManagerIdAsync_ShouldReturnCorrectTeam()
//     {
//         var db = GetDbContext();
//         var managerId = Guid.NewGuid();
//         var team = CreateSampleTeam(name: "Delta", managerId: managerId);
//         db.Teams.Add(team);
//         db.SaveChanges();
//         var repo = new TeamRepository(db);

//         var result = await repo.GetTeamByNameAndTeamManagerIdAsync("Delta", managerId);

//         Assert.NotNull(result);
//         Assert.Equal("Delta", result!.Name);
//         Assert.Equal(managerId, result.TeamManagerId);
//     }

//     [Fact]
//     public async Task GetTeamsByMemberIdAsync_ShouldReturnTeams()
//     {
//         var db = GetDbContext();
//         var memberId = Guid.NewGuid();
//         db.Teams.Add(CreateSampleTeam(members: new List<Guid> { memberId }));
//         db.Teams.Add(CreateSampleTeam(members: new List<Guid> { memberId, Guid.NewGuid() }));
//         db.Teams.Add(CreateSampleTeam(members: new List<Guid> { Guid.NewGuid() }));
//         db.SaveChanges();
//         var repo = new TeamRepository(db);

//         var result = await repo.GetTeamsByMemberIdAsync(memberId);

//         Assert.Equal(2, result.Count);
//         Assert.All(result, t => Assert.Contains(memberId, t.MembersIds));
//     }

//     [Fact]
//     public async Task GetTeamByNameAndMemberIdAsync_ShouldReturnCorrectTeam()
//     {
//         var db = GetDbContext();
//         var memberId = Guid.NewGuid();
//         var team = CreateSampleTeam(name: "Echo", members: new List<Guid> { memberId });
//         db.Teams.Add(team);
//         db.SaveChanges();
//         var repo = new TeamRepository(db);

//         var result = await repo.GetTeamByNameAndMemberIdAsync(
//             memberId,
//             "Echo",
//             CancellationToken.None
//         );

//         Assert.NotNull(result);
//         Assert.Equal("Echo", result!.Name);
//         Assert.Contains(memberId, result.MembersIds);
//     }

//     [Fact]
//     public async Task DeleteTeamAsync_ShouldRemoveTeam()
//     {
//         var db = GetDbContext();
//         var team = CreateSampleTeam();
//         db.Teams.Add(team);
//         db.SaveChanges();
//         var repo = new TeamRepository(db);

//         await repo.DeleteTeamAsync(team.Id);

//         Assert.Empty(db.Teams);
//     }

//     // [Fact]
//     // public async Task UpdateTeamAsync_ShouldSaveChanges()
//     // {
//     //     var db = GetDbContext();
//     //     var team = CreateSampleTeam();
//     //     db.Teams.Add(team);
//     //     db.SaveChanges();
//     //     var repo = new TeamRepository(db);

//     //     team.Name = "Updated";
//     //     await repo.UpdateTeamAsync(team);

//     //     var updated = db.Teams.First();
//     //     Assert.Equal("Updated", updated.Name);
//     // }

//     [Fact]
//     public async Task SaveAsync_ShouldPersistChanges()
//     {
//         var db = GetDbContext();
//         var repo = new TeamRepository(db);
//         var team = CreateSampleTeam();

//         db.Teams.Add(team);
//         await repo.SaveAsync();

//         Assert.Single(db.Teams);
//     }
// }
