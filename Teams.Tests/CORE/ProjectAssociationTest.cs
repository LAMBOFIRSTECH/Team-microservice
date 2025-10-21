using System;
using System.Collections.Generic;
using Teams.CORE.Layer.Entities.GeneralValueObjects;
using Teams.CORE.Layer.Entities.TeamAggregate.TeamValueObjects;
using Xunit;

namespace Teams.Tests.CORE;

public class ProjectAssociationTest
{
    [Fact]
    public void Detail_Should_SetPropertiesCorrectly()
    {
        // Arrange
        var projectName = "Project A";
        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 12, 31);
        var state = VoState.Active;

        // Act
        var detail = new Detail(projectName, startDate, endDate, state);

        // Assert
        Assert.Equal(projectName, detail.ProjectName);
        Assert.Equal(startDate, detail.ProjectStartDate);
        Assert.Equal(endDate, detail.ProjectEndDate);
        Assert.Equal(state, detail.State);
    }

    [Fact]
    public void ProjectAssociation_Should_SetPropertiesCorrectly()
    {
        // Arrange
        var teamManagerId = Guid.NewGuid();
        var teamName = "Team Alpha";
        var details = new List<Detail>
        {
            new Detail("Project A", DateTime.Now, DateTime.Now.AddMonths(6), VoState.Active),
            new Detail(
                "Project B",
                DateTime.Now,
                DateTime.Now.AddMonths(3),
                VoState.Suspended
            ),
        };

        // Act
        var association = new ProjectAssociation(teamManagerId, teamName, details);

        // Assert
        Assert.Equal(teamManagerId, association.TeamManagerId);
        Assert.Equal(teamName, association.TeamName);
        Assert.Equal(details, association.Details);
        Assert.Equal(2, association.Details.Count);
    }

    [Fact]
    public void ProjectAssociation_DetailsListIsImmutableReference()
    {
        // Arrange
        var details = new List<Detail>();
        var association = new ProjectAssociation(Guid.NewGuid(), "Team Beta", details);

        // Act
        details.Add(
            new Detail("Project X", DateTime.Now, DateTime.Now.AddMonths(1), VoState.Active)
        );

        // Assert
        Assert.Equal(1, association.Details.Count); // Same reference
    }
}
