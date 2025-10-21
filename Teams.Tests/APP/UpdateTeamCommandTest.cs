using System;
using System.Collections.Generic;
using FluentAssertions;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Commands;
using Xunit;

namespace Teams.Tests.APP;

public class UpdateTeamCommandTests
{
    [Fact]
    public void DefaultConstructor_Initializes_MemberId_As_EmptySet()
    {
        // Arrange
        var command = new UpdateTeamCommand();

        // Assert
        command.MemberId.Should().NotBeNull();
        command.MemberId.Should().BeEmpty();
    }

    [Fact]
    public void ParameterizedConstructor_Sets_All_Properties_Correctly()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var teamName = "New Dev Team";
        var teamManagerId = Guid.NewGuid();
        var members = new HashSet<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        var command = new UpdateTeamCommand(teamId, teamName, teamManagerId, members);

        // Assert
        command.Id.Should().Be(teamId);
        command.Name.Should().Be(teamName);
        command.TeamManagerId.Should().Be(teamManagerId);
        command.MemberId.Should().BeEquivalentTo(members);
    }

    [Fact]
    public void Can_Set_Name_Property()
    {
        var command = new UpdateTeamCommand();
        command.Name = "Updated Team Name";
        command.Name.Should().Be("Updated Team Name");
    }

    [Fact]
    public void Can_Set_TeamManagerId_Property()
    {
        var teamManagerId = Guid.NewGuid();
        var command = new UpdateTeamCommand();
        command.TeamManagerId = teamManagerId;
        command.TeamManagerId.Should().Be(teamManagerId);
    }

    [Fact]
    public void Can_Add_Members_To_MemberId()
    {
        var command = new UpdateTeamCommand();
        var memberId = Guid.NewGuid();

        command.MemberId.Add(memberId);

        command.MemberId.Should().ContainSingle().And.Contain(memberId);
    }

    [Fact]
    public void MemberId_Is_Modifiable()
    {
        var command = new UpdateTeamCommand();
        var memberId = Guid.NewGuid();

        command.MemberId.Add(memberId);
        command.MemberId.Should().Contain(memberId);

        var newMemberId = Guid.NewGuid();
        command.MemberId.Add(newMemberId);
        command.MemberId.Should().Contain(newMemberId);
    }
}
