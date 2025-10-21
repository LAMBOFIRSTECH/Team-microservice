// using System;
// using System.Collections.Generic;
// using Teams.API.Layer.DTOs;
// using Xunit;

// namespace Teams.Tests.API;

// public class TeamRequestDtoTest
// {
//     [Fact]
//     public void DefaultConstructor_Should_InitializeMemberId()
//     {
//         // Act
//         var dto = new TeamRequestDto();

//         // Assert
//         Assert.NotNull(dto.MemberId);
//         Assert.Empty(dto.MemberId);
//     }

//     [Fact]
//     public void Constructor_WithMembersIncluded_Should_SetProperties()
//     {
//         // Arrange
//         var id = Guid.NewGuid();
//         var managerId = Guid.NewGuid();
//         var teamName = "Alpha Team";
//         var memberIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

//         // Act
//         var dto = new TeamRequestDto(
//             id,
//             managerId,
//             teamName,
//             includeMembers: true,
//             memberIds: memberIds
//         );

//         // Assert
//         Assert.Equal(id, dto.Id);
//         Assert.Equal(managerId, dto.TeamManagerId);
//         Assert.Equal(teamName, dto.Name);
//         Assert.Equal(memberIds, dto.MemberId);
//     }

//     [Fact]
//     public void Constructor_WithoutMembersIncluded_Should_SetEmptyMemberId()
//     {
//         // Arrange
//         var id = Guid.NewGuid();
//         var managerId = Guid.NewGuid();
//         var teamName = "Beta Team";

//         // Act
//         var dto = new TeamRequestDto(id, managerId, teamName);

//         // Assert
//         Assert.Equal(id, dto.Id);
//         Assert.Equal(managerId, dto.TeamManagerId);
//         Assert.Equal(teamName, dto.Name);
//         Assert.Empty(dto.MemberId);
//     }

//     [Fact]
//     public void Constructor_WithNullMembersList_Should_InitializeEmptyMemberId()
//     {
//         // Arrange
//         var id = Guid.NewGuid();
//         var managerId = Guid.NewGuid();
//         var teamName = "Gamma Team";

//         // Act
//         var dto = new TeamRequestDto(
//             id,
//             managerId,
//             teamName,
//             includeMembers: true,
//             memberIds: null
//         );

//         // Assert
//         Assert.NotNull(dto.MemberId);
//         Assert.Empty(dto.MemberId);
//     }
// }
