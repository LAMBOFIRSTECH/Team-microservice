using System;
using Newtonsoft.Json;
using Teams.API.Layer.DTOs;
using Xunit;

namespace Teams.Tests.API
{
    public class DeleteTeamMemberDtoTest
    {
        [Fact]
        public void Constructor_Should_SetPropertiesCorrectly()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var teamName = "Team Alpha";

            // Act
            var dto = new DeleteTeamMemberDto(memberId, teamName);

            // Assert
            Assert.Equal(memberId, dto.MemberId);
            Assert.Equal(teamName, dto.TeamName);
        }

        [Fact]
        public void JsonSerialization_Should_RequireProperties()
        {
            // Arrange
            var json = "{\"MemberId\":\"" + Guid.NewGuid() + "\"}"; // Missing TeamName

            // Act & Assert
            Assert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<DeleteTeamMemberDto>(json);
            });
        }

        [Fact]
        public void JsonSerialization_Should_DeserializeCorrectly()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var teamName = "Team Beta";
            var json = JsonConvert.SerializeObject(new DeleteTeamMemberDto(memberId, teamName));

            // Act
            var dto = JsonConvert.DeserializeObject<DeleteTeamMemberDto>(json);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(memberId, dto!.MemberId);
            Assert.Equal(teamName, dto.TeamName);
        }
    }
}
