using System;
using FluentAssertions;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Queries;
using Xunit;

namespace Teams.Tests.APP;

public class GetTeamQueryTest
{
    [Fact]
    public void Constructor_Sets_Id_Property_Correctly()
    {
        // Arrange
        var teamId = Guid.NewGuid();

        // Act
        var query = new GetTeamQuery(teamId);

        // Assert
        query.Id.Should().Be(teamId);
    }

    [Fact]
    public void Id_Should_Be_Set_Only_Through_Constructor()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var query = new GetTeamQuery(teamId);

        // Assert
        query.Id.Should().Be(teamId);
        // No setter for Id: it should be set only through constructor.
    }
}
