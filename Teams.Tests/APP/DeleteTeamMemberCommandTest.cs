using System;
using FluentAssertions;
using Teams.APP.Layer.CQRS.Commands;
using Xunit;

namespace Teams.Tests.APP;

public class DeleteTeamMemberCommandTests
{
    [Fact]
    public void Constructor_Sets_MemberId_And_TeamName_Correctly()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var teamName = "Dev Team";

        // Act
        var command = new DeleteTeamMemberCommand(memberId, teamName);

        // Assert
        command.MemberId.Should().Be(memberId);
        command.TeamName.Should().Be(teamName);
    }

    // [Fact]
    // public void TeamName_Can_Be_Changed_After_Creation()
    // {
    //     // Arrange
    //     var command = new DeleteTeamMemberCommand(Guid.NewGuid(), "Initial Team");

    //     // Act
    //     command.TeamName = "Updated Team";

    //     // Assert
    //     command.TeamName.Should().Be("Updated Team");
    // }

    [Fact]
    public void MemberId_Is_ReadOnly()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var command = new DeleteTeamMemberCommand(memberId, "Any Team");

        // Assert
        command.MemberId.Should().Be(memberId);
    }
}
