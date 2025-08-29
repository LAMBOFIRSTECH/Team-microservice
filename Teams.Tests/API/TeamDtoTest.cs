using System;
using System.Collections.Generic;
using Teams.API.Layer.DTOs;
using Xunit;

namespace Teams.Tests.API
{
    public class TeamDtoTest
    {
        [Fact]
        public void DefaultConstructor_Should_InitializeMembersId()
        {
            // Act
            var dto = new TeamDto();

            // Assert
            Assert.NotNull(dto.MembersId);
            Assert.Empty(dto.MembersId);
        }

        [Fact]
        public void Constructor_WithMembersIncluded_Should_SetProperties()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var teamName = "Alpha Team";
            var memberIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            var dto = new TeamDto(managerId, teamName, includeMembers: true, memberIds: memberIds);

            // Assert
            Assert.Equal(managerId, dto.TeamManagerId);
            Assert.Equal(teamName, dto.Name);
            Assert.Equal(memberIds, dto.MembersId);
        }

        [Fact]
        public void Constructor_WithoutMembersIncluded_Should_SetEmptyMembersId()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var teamName = "Beta Team";

            // Act
            var dto = new TeamDto(managerId, teamName);

            // Assert
            Assert.Equal(managerId, dto.TeamManagerId);
            Assert.Equal(teamName, dto.Name);
            Assert.Empty(dto.MembersId);
        }

        [Fact]
        public void Constructor_WithNullMembersList_Should_InitializeEmptyMembersId()
        {
            // Arrange
            var managerId = Guid.NewGuid();
            var teamName = "Gamma Team";

            // Act
            var dto = new TeamDto(managerId, teamName, includeMembers: true, memberIds: null);

            // Assert
            Assert.NotNull(dto.MembersId);
            Assert.Empty(dto.MembersId);
        }
    }
}
