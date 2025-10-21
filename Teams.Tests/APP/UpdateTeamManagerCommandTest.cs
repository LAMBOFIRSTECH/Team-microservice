using System;
using FluentAssertions;
using Teams.APP.Layer.CQRS.Commands;
using Xunit;

namespace Teams.Tests.APP;

public class UpdateTeamManagerCommandTests
{
    [Fact]
    public void Constructor_Sets_All_Properties_Correctly()
    {
        // Arrange
        var teamName = "Dev Team";
        var oldManagerId = Guid.NewGuid();
        var newManagerId = Guid.NewGuid();
        var contratType = "Permanent";

        // Act
        var command = new UpdateTeamManagerCommand(
            teamName,
            oldManagerId,
            newManagerId,
            contratType
        );

        // Assert
        command.Name.Should().Be(teamName);
        command.OldTeamManagerId.Should().Be(oldManagerId);
        command.NewTeamManagerId.Should().Be(newManagerId);
        command.ContratType.Should().Be(contratType);
    }

    // [Fact]
    // public void Can_Set_Name_Property()
    // {
    //     // Arrange
    //     var command = new UpdateTeamManagerCommand(
    //         "Initial Team",
    //         Guid.NewGuid(),
    //         Guid.NewGuid(),
    //         "Contract"
    //     );

    //     // Act
    //     command.Name = "Updated Team Name";

    //     // Assert
    //     command.Name.Should().Be("Updated Team Name");
    // }

    // [Fact]
    // public void Can_Set_ContratType_Property()
    // {
    //     // Arrange
    //     var command = new UpdateTeamManagerCommand(
    //         "Dev Team",
    //         Guid.NewGuid(),
    //         Guid.NewGuid(),
    //         "Permanent"
    //     );

    //     // Act
    //     command.ContratType = "Temporary";

    //     // Assert
    //     command.ContratType.Should().Be("Temporary");
    // }

    [Fact]
    public void OldTeamManagerId_Is_ReadOnly()
    {
        // Arrange
        var oldManagerId = Guid.NewGuid();
        var command = new UpdateTeamManagerCommand(
            "Team",
            oldManagerId,
            Guid.NewGuid(),
            "Permanent"
        );

        // Assert
        command.OldTeamManagerId.Should().Be(oldManagerId);
        // No setter available: compile-time guarantee that this property is immutable.
    }

    [Fact]
    public void NewTeamManagerId_Is_ReadOnly()
    {
        // Arrange
        var newManagerId = Guid.NewGuid();
        var command = new UpdateTeamManagerCommand(
            "Team",
            Guid.NewGuid(),
            newManagerId,
            "Permanent"
        );

        // Assert
        command.NewTeamManagerId.Should().Be(newManagerId);
        // No setter available: compile-time guarantee that this property is immutable.
    }
}
