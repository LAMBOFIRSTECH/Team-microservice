using System;
using Teams.API.Layer.DTOs;
using Xunit;

namespace Teams.Tests.API
{
    public class ChangeManagerDtoTest
    {
        [Fact]
        public void DefaultConstructor_Should_CreateInstanceWithDefaults()
        {
            // Act
            var dto = new ChangeManagerDto();

            // Assert
            Assert.Null(dto.Name);
            Assert.Equal(Guid.Empty, dto.OldTeamManagerId);
            Assert.Equal(Guid.Empty, dto.NewTeamManagerId);
        }

        [Fact]
        public void StringConstructor_Should_ParseGuidsCorrectly()
        {
            // Arrange
            var name = "Team Alpha";
            var oldId = Guid.NewGuid();
            var newId = Guid.NewGuid();

            // Act
            var dto = new ChangeManagerDto(name, oldId.ToString(), newId.ToString());

            // Assert
            Assert.Equal(name, dto.Name);
            Assert.Equal(oldId, dto.OldTeamManagerId);
            Assert.Equal(newId, dto.NewTeamManagerId);
        }

        [Fact]
        public void StringConstructor_Should_SetEmptyGuid_WhenInvalidStrings()
        {
            // Arrange
            var name = "Team Beta";

            // Act
            var dto = new ChangeManagerDto(name, "invalid-old-id", "invalid-new-id");

            // Assert
            Assert.Equal(name, dto.Name);
            Assert.Equal(Guid.Empty, dto.OldTeamManagerId);
            Assert.Equal(Guid.Empty, dto.NewTeamManagerId);
        }

        [Fact]
        public void GuidConstructor_Should_SetPropertiesCorrectly()
        {
            // Arrange
            var name = "Team Gamma";
            var oldId = Guid.NewGuid();
            var newId = Guid.NewGuid();

            // Act
            var dto = new ChangeManagerDto(name, oldId, newId);

            // Assert
            Assert.Equal(name, dto.Name);
            Assert.Equal(oldId, dto.OldTeamManagerId);
            Assert.Equal(newId, dto.NewTeamManagerId);
        }
    }
}
