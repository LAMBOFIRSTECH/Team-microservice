using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Teams.CORE.Layer.Entities;
using Teams.INFRA.Layer.Dispatchers;
using Teams.INFRA.Layer.Interfaces;
using Teams.INFRA.Layer.Persistence;
using Xunit;

namespace Teams.Tests.INFRA;

public class TeamDbContextTest
{
    private TeamDbContext CreateDbContext(
        Mock<ITeamStateUnitOfWork>? unitOfWorkMock = null,
        Mock<IDomainEventDispatcher>? dispatcherMock = null
    )
    {
        var options = new DbContextOptionsBuilder<TeamDbContext>()
            .UseInMemoryDatabase(databaseName: "TeamDbContextTestDb")
            .Options;

        unitOfWorkMock ??= new Mock<ITeamStateUnitOfWork>();
        dispatcherMock ??= new Mock<IDomainEventDispatcher>();

        return new TeamDbContext(options, unitOfWorkMock.Object, dispatcherMock.Object);
    }

    // [Fact]
    // public async Task SaveChangesAsync_CallsRecalculateTeamStates_ForAddedOrModifiedTeams()
    // {
    //     // Arrange
    //     var unitOfWorkMock = new Mock<ITeamStateUnitOfWork>();
    //     var dispatcherMock = new Mock<IDomainEventDispatcher>();
    //     var dbContext = CreateDbContext(unitOfWorkMock, dispatcherMock);

    //     var team = new Team(Guid.NewGuid(), "Test Team");
    //     dbContext.Teams.Add(team);

    //     // Act
    //     await dbContext.SaveChangesAsync();

    //     // Assert
    //     unitOfWorkMock.Verify(
    //         u => u.RecalculateTeamStates(It.Is<IList<Team>>(l => l.Contains(team))),
    //         Times.Once
    //     );
    // }

    // [Fact]
    // public async Task SaveChangesAsync_DispatchesDomainEvents_AndClearsThem()
    // {
    //     // Arrange
    //     var unitOfWorkMock = new Mock<ITeamStateUnitOfWork>();
    //     var dispatcherMock = new Mock<IDomainEventDispatcher>();
    //     var dbContext = CreateDbContext(unitOfWorkMock, dispatcherMock);

    //     var team = new Team { Id = 2, Name = "Team With Events" };
    //     var domainEvent = new Mock<Teams.CORE.Layer.CoreEvents.IDomainEvent>().Object;
    //     // Use a method to add domain events, e.g. AddDomainEvent
    //     team.AddDomainEvent(domainEvent);
    //     dbContext.Teams.Add(team);

    //     // Act
    //     await dbContext.SaveChangesAsync();

    //     // Assert
    //     dispatcherMock.Verify(
    //         d =>
    //             d.DispatchAsync(
    //                 It.Is<IEnumerable<Teams.CORE.Layer.CoreEvents.IDomainEvent>>(ev => ev.Contains(domainEvent)),
    //                 It.IsAny<CancellationToken>()
    //             ),
    //         Times.Once
    //     );
    //     Assert.Empty(team.DomainEvents);
    // }

    // [Fact]
    // public void OnModelCreating_AppliesTeamConfiguration_AndIgnoresDomainEvents()
    // {
    //     // Arrange
    //     var dbContext = CreateDbContext();

    //     // Act
    //     var model = dbContext.Model;
    //     var teamEntity = model.FindEntityType(typeof(Team));

    //     // Assert
    //     Assert.NotNull(teamEntity);
    //     Assert.DoesNotContain(teamEntity.GetProperties(), p => p.Name == "DomainEvents");
    // }
}
