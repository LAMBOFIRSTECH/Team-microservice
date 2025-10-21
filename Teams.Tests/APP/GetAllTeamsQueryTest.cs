using System;
using System.Collections.Generic;
using FluentAssertions;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Queries;
using Xunit;

namespace Teams.Tests.APP;

public class GetAllTeamsQueryTest
{
    [Fact]
    public void DefaultConstructor_Initializes_Properties_Correctly()
    {
        // Act
        var query = new GetAllTeamsQuery();

        // Assert
        query.Id.Should().Be(Guid.Empty);
        query.TeamManagerId.Should().Be(Guid.Empty);
        query.Name.Should().BeEmpty();
        query.MemberId.Should().BeEmpty();
        query.OnlyMature.Should().BeFalse();
    }

    [Fact]
    public void ParameterizedConstructor_Sets_All_Properties_Correctly()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var teamManagerId = Guid.NewGuid();
        var memberIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var teamName = "Team A";
        var onlyMature = true;

        // Act
        var query = new GetAllTeamsQuery(teamId, teamManagerId, memberIds, teamName, onlyMature);

        // Assert
        query.Id.Should().Be(teamId);
        query.TeamManagerId.Should().Be(teamManagerId);
        query.MemberId.Should().BeEquivalentTo(memberIds);
        query.Name.Should().Be(teamName);
        query.OnlyMature.Should().Be(onlyMature);
    }

    [Fact]
    public void Can_Set_Name_Property()
    {
        // Arrange
        var query = new GetAllTeamsQuery();

        // Act
        query.Name = "Updated Team Name";

        // Assert
        query.Name.Should().Be("Updated Team Name");
    }

    [Fact]
    public void Can_Set_OnlyMature_Property()
    {
        // Arrange
        var query = new GetAllTeamsQuery();

        // Act
        query.OnlyMature = true;

        // Assert
        query.OnlyMature.Should().BeTrue();
    }

    // [Fact]
    // public void Can_Add_Members_To_MemberId()
    // {
    //     // Arrange
    //     var query = new GetAllTeamsQuery();

    //     // Act
    //     query.MemberId.Add(Guid.NewGuid());
    //     query.MemberId.Add(Guid.NewGuid());

    //     // Assert
    //     query.MemberId.Should().HaveCount(2);
    // }
}
