// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Caching.Distributed;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Newtonsoft.Json;
// using Teams.CORE.Layer.Entities;
// using Teams.CORE.Layer.Interfaces;
// using Teams.INFRA.Layer.ExternalServices;
// using Xunit;

// namespace Teams.Tests.INFRA;

// public class RedisCacheServiceTests
// {
//     private readonly Mock<IDistributedCache> _cacheMock;
//     private readonly Mock<ITeamRepository> _teamRepoMock;
//     private readonly Mock<ILogger<RedisCacheService>> _loggerMock;
//     private readonly RedisCacheService _service;

//     public RedisCacheServiceTests()
//     {
//         _cacheMock = new Mock<IDistributedCache>();
//         _teamRepoMock = new Mock<ITeamRepository>();
//         _loggerMock = new Mock<ILogger<RedisCacheService>>();
//         _service = new RedisCacheService(
//             _cacheMock.Object,
//             _teamRepoMock.Object,
//             _loggerMock.Object
//         );
//     }

//     [Fact]
//     public async Task StoreArchivedTeamInRedisAsync_ShouldStoreTeamAndDeleteFromRepo_WhenKeyDoesNotExist()
//     {
//         // Arrange
//         var team = new Team { Id = Guid.NewGuid(), Name = "Dev Team" };
//         _cacheMock
//             .Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync((string)null);

//         // Act
//         await _service.StoreArchivedTeamInRedisAsync(team);

//         // Assert
//         _cacheMock.Verify(
//             c =>
//                 c.SetStringAsync(
//                     It.Is<string>(k => k.Contains(team.Id.ToString())),
//                     It.Is<string>(v => v.Contains(team.Name)),
//                     It.IsAny<DistributedCacheEntryOptions>(),
//                     It.IsAny<CancellationToken>()
//                 ),
//             Times.Once
//         );

//         _teamRepoMock.Verify(
//             r => r.DeleteTeamAsync(team.Id, It.IsAny<CancellationToken>()),
//             Times.Once
//         );
//     }

//     [Fact]
//     public async Task StoreArchivedTeamInRedisAsync_ShouldThrow_WhenKeyAlreadyExists()
//     {
//         // Arrange
//         var team = new Team { Id = Guid.NewGuid(), Name = "Dev Team" };
//         _cacheMock
//             .Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync("already exists");

//         // Act & Assert
//         await Assert.ThrowsAsync<InvalidOperationException>(() =>
//             _service.StoreArchivedTeamInRedisAsync(team)
//         );
//     }

//     [Fact]
//     public async Task StoreNewTeamMemberInformationsInRedisAsync_ShouldStoreMemberData_WhenKeyDoesNotExist()
//     {
//         // Arrange
//         var memberId = Guid.NewGuid();
//         var teamName = "Dev Team";
//         _cacheMock
//             .Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync((string?)null);

//         // Act
//         await _service.StoreNewTeamMemberInformationsInRedisAsync(memberId, teamName);

//         // Assert
//         _cacheMock.Verify(
//             c =>
//                 c.SetStringAsync(
//                     It.Is<string>(k => k.Contains(memberId.ToString())),
//                     It.Is<string>(v => v.Contains(teamName)),
//                     It.IsAny<DistributedCacheEntryOptions>(),
//                     It.IsAny<CancellationToken>() // <-- ici
//                 ),
//             Times.Once
//         );
//     }

//     [Fact]
//     public async Task GetNewTeamMemberFromCacheAsync_ShouldReturnTeamNameAndRemoveKey_WhenKeyExists()
//     {
//         // Arrange
//         var memberId = Guid.NewGuid();
//         var teamName = "Dev Team";
//         var cacheKey = $"DevCache:{memberId}";
//         var cachedData = JsonConvert.SerializeObject(
//             new Dictionary<string, object> { { "Id member", memberId }, { "Team Name", teamName } }
//         );

//         _cacheMock
//             .Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(cachedData);

//         // Act
//         var result = await _service.GetNewTeamMemberFromCacheAsync(memberId);

//         // Assert
//         Assert.Equal(teamName, result);
//         _cacheMock.Verify(
//             c => c.RemoveAsync(It.Is<string>(k => k == cacheKey), It.IsAny<CancellationToken>()),
//             Times.Once
//         );
//     }

//     [Fact]
//     public async Task GetNewTeamMemberFromCacheAsync_ShouldReturnEmpty_WhenKeyDoesNotExist()
//     {
//         // Arrange
//         var memberId = Guid.NewGuid();
//         _cacheMock
//             .Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync((string)null!);

//         // Act
//         var result = await _service.GetNewTeamMemberFromCacheAsync(memberId);

//         // Assert
//         Assert.Equal(string.Empty, result);
//     }

//     [Fact]
//     public async Task GetNewTeamMemberFromCacheAsync_ShouldThrow_WhenTeamNameMissing()
//     {
//         // Arrange
//         var memberId = Guid.NewGuid();
//         var cachedData = JsonConvert.SerializeObject(
//             new Dictionary<string, object> { { "Id member", memberId } }
//         );

//         _cacheMock
//             .Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//             .ReturnsAsync(cachedData);

//         // Act & Assert
//         await Assert.ThrowsAsync<InvalidOperationException>(() =>
//             _service.GetNewTeamMemberFromCacheAsync(memberId)
//         );
//     }
// }
