using System;
using System.Collections.Generic;
using Teams.CORE.Layer.BusinessExceptions;
using Teams.CORE.Layer.Entities;
using Teams.CORE.Layer.ValueObjects;
using Xunit;

namespace Teams.Tests
{
    public class TestDomain
    {
        private Guid managerId = Guid.NewGuid();
        private Guid member1 = Guid.NewGuid();
        private Guid member2 = Guid.NewGuid();
        private Guid member3 = Guid.NewGuid();

        private List<Guid> GetMembers() => new List<Guid> { managerId, member1, member2 };

        private Team CreateValidTeam()
        {
            return Team.Create("Alpha", managerId, GetMembers(), new List<Team>(), false);
        }

        [Fact]
        public void Create_ValidTeam_Succeeds()
        {
            var team = Team.Create("Alpha", managerId, GetMembers(), new List<Team>(), false);
            Assert.Equal("Alpha", team.Name);
            Assert.Equal(managerId, team.TeamManagerId);
            Assert.Equal(TeamState.Active, team.State);
            Assert.Contains(managerId, team.MembersIds);
        }

        [Fact]
        public void Create_TeamWithDuplicateName_Throws()
        {
            var existing = Team.Create("Alpha", managerId, GetMembers(), new List<Team>(), false);
            var teams = new List<Team> { existing };
            Assert.Throws<DomainException>(() =>
                Team.Create("Alpha", managerId, GetMembers(), teams, false)
            );
        }

        [Fact]
        public void Create_TeamWithTooFewMembers_Throws()
        {
            var members = new List<Guid> { managerId };
            Assert.Throws<DomainException>(() =>
                Team.Create("Beta", managerId, members, new List<Team>(), false)
            );
        }

        [Fact]
        public void AddMember_AlreadyExists_Throws()
        {
            var team = CreateValidTeam();
            Assert.Throws<DomainException>(() => team.AddMember(managerId));
        }

        [Fact]
        public void AddMember_NewMember_Succeeds()
        {
            var team = CreateValidTeam();
            team.AddMember(member3);
            Assert.Contains(member3, team.MembersIds);
        }

        [Fact]
        public void DeleteTeamMemberSafely_Manager_Throws()
        {
            var team = CreateValidTeam();
            Assert.Throws<DomainException>(() => team.DeleteTeamMemberSafely(managerId));
        }

        [Fact]
        public void DeleteTeamMemberSafely_NotFound_Throws()
        {
            var team = CreateValidTeam();
            var notMember = Guid.NewGuid();
            Assert.Throws<DomainException>(() => team.DeleteTeamMemberSafely(notMember));
        }

        [Fact]
        public void DeleteTeamMemberSafely_ValidMember_RemovesMember()
        {
            var team = CreateValidTeam();
            team.DeleteTeamMemberSafely(member1);
            Assert.DoesNotContain(member1, team.MembersIds);
        }

        [Fact]
        public void Suspend_ActiveTeam_SetsState()
        {
            var team = CreateValidTeam();
            team.Suspend();
            Assert.Equal(TeamState.Suspendue, team.State);
        }

        // [Fact]
        // public void Suspend_NotActive_Throws()
        // {
        //     var team = CreateValidTeam();
        //     team.State = TeamState.Complete;
        //     Assert.Throws<DomainException>(() => team.Suspend());
        // }

        [Fact]
        public void ChangeTeamManager_NotMember_Throws()
        {
            var team = CreateValidTeam();
            var notMember = Guid.NewGuid();
            Assert.Throws<DomainException>(() => team.ChangeTeamManager(notMember));
        }

        [Fact]
        public void ChangeTeamManager_Valid_ChangesManager()
        {
            var team = CreateValidTeam();
            team.ChangeTeamManager(member1);
            Assert.Equal(member1, team.TeamManagerId);
        }

        [Fact]
        public void GetCommonMembersStats_NoExistingTeams_ReturnsZero()
        {
            var percent = Team.GetCommonMembersStats(GetMembers(), new List<Team>());
            Assert.Equal(0, percent);
        }

        [Fact]
        public void GetCommonMembersStats_ThrowsIfNoMembers()
        {
            Assert.Throws<DomainException>(() =>
                Team.GetCommonMembersStats(new List<Guid>(), new List<Team>())
            );
        }

        [Fact]
        public void UpdateTeam_NoChanges_Throws()
        {
            var team = CreateValidTeam();
            Assert.Throws<DomainException>(() =>
                team.UpdateTeam(team.Name, team.TeamManagerId, team.MembersIds)
            );
        }

        [Fact]
        public void UpdateTeam_Valid_UpdatesTeam()
        {
            var team = CreateValidTeam();
            var newName = "Beta";
            var newManager = member1;
            var newMembers = new List<Guid> { newManager, member2, member3 };
            team.UpdateTeam(newName, newManager, newMembers);
            Assert.Equal(newName, team.Name);
            Assert.Equal(newManager, team.TeamManagerId);
            Assert.Equal(newMembers, team.MembersIds);
        }
        [Fact]
        public void TurnToArchiveIfInactive_SetsStateIfInactive()
        {
            var team = CreateValidTeam();
            team.LastActivityDate = DateTime.UtcNow.AddDays(-91);
            team.TurnToArchiveIfInactive();
            Assert.Equal(TeamState.Archivee, team.State);
        }

        // [Fact]
        // public void AttachProjectToTeam_ActiveAssociatedProject_SetsStateComplete()
        // {
        //     var team = CreateValidTeam();
        //     var projectAssociation = new ProjectAssociation
        //     {
        //         TeamManagerId = team.TeamManagerId,
        //         TeamName = team.Name,
        //         ProjectStartDate = DateTime.UtcNow,
        //     };
        //     team.State = TeamState.Active;
        //     team.AttachProjectToTeam(projectAssociation, true);
        //     Assert.Equal(TeamState.Complete, team.State);
        // }
    }

    // Dummy ProjectAssociation for testing
    public class ProjectAssociation
    {
        public Guid TeamManagerId { get; set; }
        public string? TeamName { get; set; }
        public DateTime ProjectStartDate { get; set; }
    }
}
