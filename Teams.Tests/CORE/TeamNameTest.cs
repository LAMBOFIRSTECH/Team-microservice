using System;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;
using Xunit;

namespace Teams.Tests.CORE;

public class TeamNameTest
{
    [Theory]
    [InlineData("Development Team")]
    [InlineData("Équipe Internationale")]
    [InlineData("O'Connor Team")]
    [InlineData("Team-A")]
    [InlineData("  Leading and trailing spaces  ")]
    public void Create_ShouldReturnTeamName_WhenValueIsValid(string validName)
    {
        // Act
        var teamName = TeamName.Create(validName);

        // Assert
        Assert.NotNull(teamName);
        Assert.Equal(validName.Trim(), teamName.Value);
        Assert.Equal(validName.Trim(), teamName.ToString());
    }

    // [Theory]
    // [InlineData(null)]
    // [InlineData("")]
    // [InlineData("   ")]
    // public void Create_ShouldThrowArgumentException_WhenValueIsNullOrEmpty(string invalidName)
    // {
    //     // Act & Assert
    //     var ex = Assert.Throws<ArgumentException>(() => TeamName.Create(invalidName));
    //     Assert.Equal("Team name cannot be empty.", ex.Message);
    // }

    // [Theory]
    // [InlineData("Team123")]
    // [InlineData("Team!")]
    // [InlineData("Team@Name")]
    // [InlineData("Team#Name")]
    // public void Create_ShouldThrowArgumentException_WhenValueContainsInvalidCharacters(
    //     string invalidName
    // )
    // {
    //     // Act & Assert
    //     var ex = Assert.Throws<ArgumentException>(() => TeamName.Create(invalidName));
    //     Assert.Equal("Team name contains invalid characters.", ex.Message);
    // }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var teamName = TeamName.Create("My Team");

        // Act
        var str = teamName.ToString();

        // Assert
        Assert.Equal("My Team", str);
    }
}
