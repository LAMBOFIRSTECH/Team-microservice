using System;
using FluentAssertions;
using Teams.APP.Layer.CQRS.Commands;
using Xunit;

namespace Teams.Tests.APP;

public class DeleteTeamCommandTest
{
    [Fact]
    public void Constructor_Sets_Id_And_Name_Correctly()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var teamName = "Team To Delete";

        // Act
        var command = new DeleteTeamCommand(teamId, teamName);

        // Assert
        command.Id.Should().Be(teamId);
        command.Name.Should().Be(teamName);
    }

    [Fact]
    public void Name_Can_Be_Changed_After_Creation()
    {
        // Arrange
        var command = new DeleteTeamCommand(Guid.NewGuid(), "Initial Name");

        // Act
        command.Name = "Updated Name";

        // Assert
        command.Name.Should().Be("Updated Name");
    }

    [Fact]
    public void Id_Is_ReadOnly()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var command = new DeleteTeamCommand(teamId, "Any Name");

        // Assert
        command.Id.Should().Be(teamId);
        // No setter available: compile-time guarantee.
    }
}
