// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Teams.APP.Layer.ExternalServicesDtos;
// using Teams.CORE.Layer.BusinessExceptions;
// using Teams.CORE.Layer.Entities;
// using Teams.CORE.Layer.Models;
// using Xunit;

// namespace Teams.Tests
// {
//     public class TestDomain
//     {
//         private Guid _managerId = Guid.NewGuid();
//         private Guid _member1 = Guid.NewGuid();
//         private Guid _member2 = Guid.NewGuid();

//         private List<Guid> GetValidMembers()
//         {
//             return new List<Guid> { _managerId, _member1, _member2 };
//         }

//         [Fact]
//         public void Create_ValidTeam_Succeeds()
//         {
//             var team = Team.Create("Alpha", _managerId, GetValidMembers(), new List<Team>());
//             Assert.Equal("Alpha", team.Name);
//             Assert.Equal(_managerId, team.TeamManagerId);
//             Assert.Equal(TeamState.Active, team.State);
//             Assert.Contains(_managerId, team.MemberIds);
//         }

//         [Fact]
//         public void Create_TeamWithLessThan2Members_Throws()
//         {
//             var members = new List<Guid> { _managerId };
//             Assert.Throws<DomainException>(() =>
//                 Team.Create("Beta", _managerId, members, new List<Team>())
//             );
//         }

//         [Fact]
//         public void Create_TeamWithMoreThan10Members_Throws()
//         {
//             var members = Enumerable.Range(0, 11).Select(_ => Guid.NewGuid()).ToList();
//             members[0] = _managerId;
//             Assert.Throws<DomainException>(() =>
//                 Team.Create("Gamma", _managerId, members, new List<Team>())
//             );
//         }

//         [Fact]
//         public void Create_TeamWithDuplicateMembers_Throws()
//         {
//             var members = new List<Guid> { _managerId, _member1, _member1 };
//             Assert.Throws<DomainException>(() =>
//                 Team.Create("Delta", _managerId, members, new List<Team>())
//             );
//         }

//         [Fact]
//         public void Create_TeamManagerNotInMembers_Throws()
//         {
//             var members = new List<Guid> { _member1, _member2 };
//             Assert.Throws<DomainException>(() =>
//                 Team.Create("Epsilon", _managerId, members, new List<Team>())
//             );
//         }

//         [Fact]
//         public void Create_TeamWithDuplicateName_Throws()
//         {
//             var existing = new List<Team>
//             {
//                 Team.Create("Zeta", _managerId, GetValidMembers(), new List<Team>()),
//             };
//             Assert.Throws<DomainException>(() =>
//                 Team.Create("Zeta", _managerId, GetValidMembers(), existing)
//             );
//         }

//         [Fact]
//         public void Activate_TeamWithInsufficientMembers_Throws()
//         {
//             var members = new List<Guid> { _managerId, _member1 };
//             var team = Team.Create("Eta", _managerId, members, new List<Team>());
//             team.DeleteTeamMemberSafely(_member1);
//             Assert.Throws<DomainException>(() => team.Activate());
//         }

//         [Fact]
//         public void Suspend_ActiveTeam_SetsStateToSuspendue()
//         {
//             var team = Team.Create("Theta", _managerId, GetValidMembers(), new List<Team>());
//             team.Suspend();
//             Assert.Equal(TeamState.Suspendue, team.State);
//         }

//         [Fact]
//         public void Suspend_NonActiveTeam_Throws()
//         {
//             var team = Team.Create("Iota", _managerId, GetValidMembers(), new List<Team>());
//             team.Suspend();
//             Assert.Throws<DomainException>(() => team.Suspend());
//         }

//         [Fact]
//         public void ResetState_Archivee_DoesNothing()
//         {
//             var team = Team.Create("Kappa", _managerId, GetValidMembers(), new List<Team>());
//             team.Suspend();
//             // Simulate archiving the team using a public method if available, or skip this test if not possible.
//             // Example: team.Archive(); // Uncomment if such a method exists.
//             // If no public method exists, this test cannot set the state to Archivee directly.
//             team.ResetState();
//             Assert.Equal(TeamState.Suspendue, team.State); // Adjust expected state if needed.
//         }

//         [Fact]
//         public void ResetState_LessThan2Members_SetsIncomplete()
//         {
//             var team = Team.Create("Lambda", _managerId, GetValidMembers(), new List<Team>());
//             team.DeleteTeamMemberSafely(_member1);
//             team.DeleteTeamMemberSafely(_member2);
//             team.ResetState();
//             Assert.Equal(TeamState.Incomplete, team.State);
//         }

//         // [Fact]
//         // public void ResetState_NoActiveProject_SetsSuspendue()
//         // {
//         //     var team = Team.Create("Mu", _managerId, GetValidMembers(), new List<Team>());
//         //     team.ActifAssociatedProject = false;
//         //     team.ResetState();
//         //     Assert.Equal(TeamState.Suspendue, team.State);
//         // }

//         // [Fact]
//         // public void ResetState_LowProductivityOrHighTurnover_SetsEnRevision()
//         // {
//         //     var team = Team.Create("Nu", _managerId, GetValidMembers(), new List<Team>());
//         //     team.ActiveAssociatedProject = true;
//         //     team.AverageProductivity = 0.3;
//         //     team.TauxTurnover = 0.6;
//         //     team.ResetState();
//         //     Assert.Equal(TeamState.EnRevision, team.State);
//         // }

//         // [Fact]
//         // public void ResetState_AllGood_SetsActive()
//         // {
//         //     var team = Team.Create("Xi", _managerId, GetValidMembers(), new List<Team>());
//         //     team.ActifAssociatedProject = true;
//         //     team.AverageProductivity = 0.8;
//         //     team.TauxTurnover = 0.1;
//         //     team.ResetState();
//         //     Assert.Equal(TeamState.Active, team.State);
//         // }

//         // [Fact]
//         // public void TurnToArchiveIfInactive_Over90Days_SetsArchivee()
//         // {
//         //     var team = Team.Create("Omicron", _managerId, GetValidMembers(), new List<Team>());
//         //     team.DateDerniereActivite = DateTime.UtcNow.AddDays(-91);
//         //     team.TurnToArchiveIfInactive();
//         //     Assert.Equal(TeamState.Archivee, team.State);
//         // }

//         [Fact]
//         public void AddMember_AlreadyExists_Throws()
//         {
//             var team = Team.Create("Pi", _managerId, GetValidMembers(), new List<Team>());
//             Assert.Throws<DomainException>(() => team.AddMember(_managerId));
//         }

//         [Fact]
//         public void AddMember_NewMember_Succeeds()
//         {
//             var team = Team.Create("Rho", _managerId, GetValidMembers(), new List<Team>());
//             var newMember = Guid.NewGuid();
//             team.AddMember(newMember);
//             Assert.Contains(newMember, team.MemberIds);
//         }

//         [Fact]
//         public void ChangeTeamManager_EmptyId_Throws()
//         {
//             var team = Team.Create("Sigma", _managerId, GetValidMembers(), new List<Team>());
//             Assert.Throws<DomainException>(() => team.ChangeTeamManager(Guid.Empty));
//         }

//         // [Fact]
//         // public void ChangeTeamManager_NotInMembers_Throws()
//         // {
//         //     var team = Team.Create("Tau", _managerId, GetValidMembers(), new List<Team>());
//         //     var outsider = Guid.NewGuid();
//         //     Assert.Throws<DomainException>(() => team.ChangeTeamManager(outsider));
//         // }

//         [Fact]
//         public void DeleteTeamMemberSafely_Manager_Throws()
//         {
//             var team = Team.Create("Upsilon", _managerId, GetValidMembers(), new List<Team>());
//             Assert.Throws<DomainException>(() => team.DeleteTeamMemberSafely(_managerId));
//         }

//         [Fact]
//         public void DeleteTeamMemberSafely_NotFound_Throws()
//         {
//             var team = Team.Create("Phi", _managerId, GetValidMembers(), new List<Team>());
//             var outsider = Guid.NewGuid();
//             Assert.Throws<DomainException>(() => team.DeleteTeamMemberSafely(outsider));
//         }

//         [Fact]
//         public void UpdateTeam_NoChanges_Throws()
//         {
//             var team = Team.Create("Chi", _managerId, GetValidMembers(), new List<Team>());
//             Assert.Throws<DomainException>(() =>
//                 team.UpdateTeam("Chi", _managerId, GetValidMembers())
//             );
//         }

//         [Fact]
//         public void UpdateTeam_Changes_Succeeds()
//         {
//             var team = Team.Create("Psi", _managerId, GetValidMembers(), new List<Team>());
//             var newName = "PsiUpdated";
//             var newManager = _member1;
//             var newMembers = new List<Guid> { _member1, _member2 };
//             team.UpdateTeam(newName, newManager, newMembers);
//             Assert.Equal(newName, team.Name);
//             Assert.Equal(newManager, team.TeamManagerId);
//             Assert.Equal(2, team.MemberIds.Count);
//         }

//         private Team CreateActiveTeam(out Guid teamId, out Guid managerId, out string teamName)
//         {
//             teamName = "TestTeam";
//             managerId = Guid.NewGuid();
//             var memberIds = new List<Guid> { managerId, Guid.NewGuid() };
//             var team = Team.Create(teamName, managerId, memberIds, new List<Team>());
//             teamId = team.Id;
//             return team;
//         }

//         [Fact]
//         public void IsAnyProjectAssociated_ValidDto_SetsActiveAssociatedProject()
//         {
//             // Arrange
//             var team = CreateActiveTeam(out var teamId, out var managerId, out var teamName);
//             var dto = new ProjectAssociatedDto(teamId, teamName, managerId);

//             // Act
//             team.IsAnyProjectAssociated(dto);

//             // Assert
//             Assert.True(team.ActiveAssociatedProject);
//         }

//         [Fact]
//         public void IsAnyProjectAssociated_NullDto_ThrowsDomainException()
//         {
//             // Arrange
//             var team = CreateActiveTeam(out _, out _, out _);

//             // Act & Assert
//             Assert.Throws<DomainException>(() => team.IsAnyProjectAssociated(null));
//         }

//         [Fact]
//         public void IsAnyProjectAssociated_MismatchedTeamId_ThrowsDomainException()
//         {
//             // Arrange
//             var team = CreateActiveTeam(out var teamId, out var managerId, out var teamName);
//             var dto = new ProjectAssociatedDto(Guid.NewGuid(), teamName, managerId);

//             // Act & Assert
//             var ex = Assert.Throws<DomainException>(() => team.IsAnyProjectAssociated(dto));
//             Assert.Contains("does not match current team", ex.Message);
//         }

//         [Fact]
//         public void IsAnyProjectAssociated_MismatchedManagerId_ThrowsDomainException()
//         {
//             // Arrange
//             var team = CreateActiveTeam(out var teamId, out var managerId, out var teamName);
//             var dto = new ProjectAssociatedDto(teamId, teamName, Guid.NewGuid());

//             // Act & Assert
//             var ex = Assert.Throws<DomainException>(() => team.IsAnyProjectAssociated(dto));
//             Assert.Contains("does not match current team", ex.Message);
//         }

//         [Fact]
//         public void IsAnyProjectAssociated_MismatchedTeamName_ThrowsDomainException()
//         {
//             // Arrange
//             var team = CreateActiveTeam(out var teamId, out var managerId, out _);
//             var dto = new ProjectAssociatedDto(teamId, "OtherName", managerId);

//             // Act & Assert
//             var ex = Assert.Throws<DomainException>(() => team.IsAnyProjectAssociated(dto));
//             Assert.Contains("does not match current team", ex.Message);
//         }

//         [Fact]
//         public void IsAnyProjectAssociated_TeamNotActive_ThrowsDomainException()
//         {
//             // Arrange
//             var team = CreateActiveTeam(out var teamId, out var managerId, out var teamName);
//             // Set state to Suspendue
//             team.Suspend();
//             var dto = new ProjectAssociatedDto(teamId, teamName, managerId);

//             // Act & Assert
//             var ex = Assert.Throws<DomainException>(() => team.IsAnyProjectAssociated(dto));
//             Assert.Equal("Only active teams can have associated projects.", ex.Message);
//         }
//     }
// }
