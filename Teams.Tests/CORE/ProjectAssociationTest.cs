using System;
using System.Collections.Generic;
using Teams.CORE.Layer.Entities.TeamAggregate.InternalEntities;
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

}
