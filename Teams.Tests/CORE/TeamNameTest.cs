using System;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;
using Xunit;

namespace Teams.Tests.CORE;

public class TeamNameTest
{
    // Test de la méthode Create avec un nom valide
    [Fact]
    public void Create_ShouldReturnTeamName_WhenValueIsValid()
    {
        // Arrange
        var validName = "Valid Team Name";

        // Act
        var teamName = TeamName.Create(validName);

        // Assert
        Assert.NotNull(teamName);
        Assert.Equal(validName, teamName.Value);
    }

    // Test de la méthode Create avec un nom invalide (caractères non autorisés)
    [Fact]
    public void Create_ShouldThrowArgumentException_WhenValueContainsInvalidCharacters()
    {
        // Arrange
        var invalidName = "Invalid@Team#Name";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => TeamName.Create(invalidName));
        Assert.Contains("contains invalid characters", exception.Message);
    }

    // Test de la méthode Equals pour vérifier que deux objets TeamName égaux sont considérés comme égaux
    [Fact]
    public void Equals_ShouldReturnTrue_WhenNamesAreEqual()
    {
        // Arrange
        var name1 = TeamName.Create("Team One");
        var name2 = TeamName.Create("Team One");

        // Act
        var result = name1.Equals(name2);

        // Assert
        Assert.True(result);
    }

    // Test de la méthode Equals pour vérifier que deux objets TeamName différents sont considérés comme différents
    [Fact]
    public void Equals_ShouldReturnFalse_WhenNamesAreDifferent()
    {
        // Arrange
        var name1 = TeamName.Create("Team One");
        var name2 = TeamName.Create("Team Two");

        // Act
        var result = name1.Equals(name2);

        // Assert
        Assert.False(result);
    }

    // Test de l'égalité des objets via l'opérateur '=='
    [Fact]
    public void OperatorEquals_ShouldReturnTrue_WhenNamesAreEqual()
    {
        // Arrange
        var name1 = TeamName.Create("Team One");
        var name2 = TeamName.Create("Team One");

        // Act & Assert
        Assert.True(name1 == name2);
    }

    // Test de l'inégalité des objets via l'opérateur '!='
    [Fact]
    public void OperatorNotEquals_ShouldReturnTrue_WhenNamesAreDifferent()
    {
        // Arrange
        var name1 = TeamName.Create("Team One");
        var name2 = TeamName.Create("Team Two");

        // Act & Assert
        Assert.True(name1 != name2);
    }

    // Test de la méthode ToString
    [Fact]
    public void ToString_ShouldReturnNameValue()
    {
        // Arrange
        var teamName = TeamName.Create("Team One");

        // Act
        var result = teamName.ToString();

        // Assert
        Assert.Equal("Team One", result);
    }
}


