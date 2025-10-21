// using System.Collections.Generic;
// using Moq;
// using Teams.CORE.Layer.Entities;
// using Teams.INFRA.Layer.UnitOfWork;
// using Xunit;

// namespace Teams.Tests.INFRA;

// public class TeamStateUnitOfWorkTest
// {
//     // [Fact]
//     // public void RecalculateTeamStates_CallsRecalculateState_ForNonArchivedTeams()
//     // {
//     //     // Arrange
//     //     var teamMock1 = new Mock<Team>();
//     //     teamMock1.SetupProperty(t => t.State, TeamState.Active);

//     //     var teamMock2 = new Mock<Team>();
//     //     teamMock2.SetupProperty(t => t.State, TeamState.Archivee);

//     //     var teamMock3 = new Mock<Team>();
//     //     teamMock3.SetupProperty(t => t.State, TeamState.Complete);

//     //     var teams = new List<Team> { teamMock1.Object, teamMock2.Object, teamMock3.Object };

//     //     var unitOfWork = new TeamStateUnitOfWork();

//     //     // Act
//     //     unitOfWork.RecalculateTeamStates(teams);

//     //     // Assert
//     //     teamMock1.Verify(t => t.RecalculateState(), Times.Once);
//     //     teamMock2.Verify(t => t.RecalculateState(), Times.Never);
//     //     teamMock3.Verify(t => t.RecalculateState(), Times.Once);
//     // }

//     // [Fact]
//     // public void RecalculateTeamStates_DoesNothing_WhenAllTeamsAreArchived()
//     // {
//     //     // Arrange
//     //     var teamMock1 = new Mock<Team>();
//     //     teamMock1.SetupProperty(t => t.State, TeamState.Archivee);

//     //     var teamMock2 = new Mock<Team>();
//     //     teamMock2.SetupProperty(t => t.State, TeamState.Archivee);

//     //     var teams = new List<Team> { teamMock1.Object, teamMock2.Object };

//     //     var unitOfWork = new TeamStateUnitOfWork();

//     //     // Act
//     //     unitOfWork.RecalculateTeamStates(teams);

//     //     // Assert
//     //     teamMock1.Verify(t => t.RecalculateState(), Times.Never);
//     //     teamMock2.Verify(t => t.RecalculateState(), Times.Never);
//     // }

//     [Fact]
//     public void RecalculateTeamStates_DoesNothing_WhenTeamsIsEmpty()
//     {
//         // Arrange
//         var teams = new List<Team>();
//         var unitOfWork = new TeamStateUnitOfWork();

//         // Act & Assert
//         unitOfWork.RecalculateTeamStates(teams);
//         // No exception should be thrown and nothing to verify
//     }
// }
