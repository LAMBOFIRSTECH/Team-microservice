// using System;
// using System.Collections.Generic;
// using FluentAssertions;
// using Teams.APP.Layer.CQRS.Commands;
// using Xunit;

// namespace Teams.Tests.APP;

// public class CreateTeamCommandTest
// {
//     [Fact]
//     public void DefaultConstructor_InitializesMembersId_AsEmptySet()
//     {
//         // Arrange
//         var command = new CreateTeamCommand();

//         // Assert
//         command.MembersId.Should().NotBeNull();
//         command.MembersId.Should().BeEmpty();
//     }

//     [Fact]
//     public void ParameterizedConstructor_SetsAllPropertiesCorrectly()
//     {
//         // Arrange
//         var name = "My New Team";
//         var teamManagerId = Guid.NewGuid();
//         var members = new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() };

//         // Act
//         var command = new CreateTeamCommand(name, teamManagerId, members);

//         // Assert
//         command.Name.Should().Be(name);
//         command.TeamManagerId.Should().Be(teamManagerId);
//         command.MembersId.Should().BeEquivalentTo(members);
//     }

//     [Fact]
//     public void Can_Set_Name_Property()
//     {
//         var command = new CreateTeamCommand();
//         command.Name = "Test Team";
//         command.Name.Should().Be("Test Team");
//     }

//     [Fact]
//     public void Can_Set_TeamManagerId_Property()
//     {
//         var managerId = Guid.NewGuid();
//         var command = new CreateTeamCommand();
//         command.TeamManagerId = managerId;
//         command.TeamManagerId.Should().Be(managerId);
//     }

//     [Fact]
//     public void Can_Add_Members_To_MembersId()
//     {
//         var command = new CreateTeamCommand();
//         var memberId = Guid.NewGuid();

//         command.MembersId.Add(memberId);

//         command.MembersId.Should().ContainSingle().And.Contain(memberId);
//     }
// }
