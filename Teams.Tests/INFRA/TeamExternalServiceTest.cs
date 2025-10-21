// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Net.Http;
// using System.Threading;
// using System.Threading.Tasks;
// using FluentValidation;
// using FluentValidation.Results;
// using MediatR;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Moq.Protected;
// using Newtonsoft.Json;
// using Teams.API.Layer.Controllers;
// using Teams.API.Layer.DTOs;
// using Teams.APP.Layer.CQRS.Commands;
// using Teams.APP.Layer.CQRS.Queries;
// using Teams.APP.Layer.Interfaces;
// using Teams.INFRA.Layer.ExternalServices;
// using Teams.INFRA.Layer.ExternalServicesDtos;
// using Xunit;

// namespace Teams.Tests.INFRA;

// public class TeamExternalServiceTest
// {
//     private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, string content)
//     {
//         var handlerMock = new Mock<HttpMessageHandler>();
//         handlerMock
//             .Protected()
//             .Setup<Task<HttpResponseMessage>>(
//                 "SendAsync",
//                 ItExpr.IsAny<HttpRequestMessage>(),
//                 ItExpr.IsAny<CancellationToken>()
//             )
//             .ReturnsAsync(
//                 new HttpResponseMessage
//                 {
//                     StatusCode = statusCode,
//                     Content = new StringContent(content),
//                 }
//             );
//         return new HttpClient(handlerMock.Object);
//     }

//     private static IConfiguration CreateMockConfiguration(string url, string key)
//     {
//         var configMock = new Mock<IConfiguration>();
//         configMock.Setup(c => c["ExternalsApi:Employee:Url"]).Returns(url);
//         configMock.Setup(c => c["ExternalsApi:Employee:Headers:X-Access-Key"]).Returns(key);
//         configMock.Setup(c => c["ExternalsApi:Project:Url"]).Returns(url);
//         configMock.Setup(c => c["ExternalsApi:Project:Headers:X-Access-Key"]).Returns(key);
//         return configMock.Object;
//     }

//     private static ILogger<TeamExternalService> CreateMockLogger()
//     {
//         return new Mock<ILogger<TeamExternalService>>().Object;
//     }

//     [Fact]
//     public async Task RetrieveNewMemberToAddInRedisAsync_ReturnsDto_WhenResponseIsValid()
//     {
//         var dto = new TransfertMemberDto(
//             Guid.NewGuid(),
//             "Equipe de sécurité (Security Team)",
//             "Pentester",
//             new AffectationStatus(true, "CDI", DateTime.UtcNow.AddDays(-10))
//         );
//         var recordJson = JsonConvert.SerializeObject(dto);
//         var responseJson = $"{{\"record\":{recordJson}}}";
//         var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseJson);
//         var config = CreateMockConfiguration("http://test-url", "test-key");
//         var logger = CreateMockLogger();

//         var service = new TeamExternalService(httpClient, config, logger);

//         var result = await service.RetrieveNewMemberToAddInRedisAsync();

//         Assert.NotNull(result);
//     }

//     // [Fact]
//     // public async Task RetrieveNewMemberToAddInRedisAsync_ReturnsNull_WhenNotFound()
//     // {
//     //     var httpClient = CreateMockHttpClient(HttpStatusCode.NotFound, "");
//     //     var config = CreateMockConfiguration("http://test-url", "test-key");
//     //     var logger = CreateMockLogger();

//     //     var service = new TeamExternalService(httpClient, config, logger);

//     //     var result = await service.RetrieveNewMemberToAddInRedisAsync();

//     //     Assert.Null(result);
//     // }

//     // [Fact]
//     // public async Task RetrieveMemberToDeleteAsync_ReturnsDto_WhenResponseIsValid()
//     // {
//     //     var dto = new TransfertMemberDto(
//     //         Guid.NewGuid(),
//     //         "Equipe de sécurité (Security Team)",
//     //         "Pentester",
//     //         new AffectationStatus(true, "CDI", DateTime.UtcNow.AddDays(-10))
//     //     );
//     //     var recordJson = JsonConvert.SerializeObject(dto);
//     //     var responseJson = $"{{\"record\":{recordJson}}}";
//     //     var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseJson);
//     //     var config = CreateMockConfiguration("http://test-url", "test-key");
//     //     var logger = CreateMockLogger();

//     //     var service = new TeamExternalService(httpClient, config, logger);

//     //     var result = await service.RetrieveMemberToDeleteAsync();

//     //     Assert.NotNull(result);
//     // }

//     [Fact]
//     public async Task RetrieveProjectAssociationDataAsync_ReturnsDto_WhenResponseIsValid()
//     {
//         var dto = new ProjectAssociationDto(
//             Guid.NewGuid(), // TeamManagerId
//             "SomeProjectName", // string parameter (replace with actual value if needed)
//             new List<DetailDto>() // List<DetailDto> (add items if needed)
//         );
//         var recordJson = JsonConvert.SerializeObject(dto);
//         var responseJson = $"{{\"record\":{recordJson}}}";
//         var httpClient = CreateMockHttpClient(HttpStatusCode.OK, responseJson);
//         var config = CreateMockConfiguration("http://test-url", "test-key");
//         var logger = CreateMockLogger();

//         var service = new TeamExternalService(httpClient, config, logger);

//         var result = await service.RetrieveProjectAssociationDataAsync();

//         Assert.NotNull(result);
//     }

//     [Fact]
//     public void UtcDateTimeConverter_ReadJson_ConvertsToUtc()
//     {
//         var converter = new TeamExternalService.UtcDateTimeConverter();
//         var date = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Local);
//         var reader = new Mock<JsonReader>();
//         reader.Setup(r => r.Value).Returns(date);

//         var result = converter.ReadJson(
//             reader.Object,
//             typeof(DateTime),
//             default,
//             false,
//             new JsonSerializer()
//         );

//         Assert.Equal(DateTimeKind.Utc, result.Kind);
//     }

//     [Fact]
//     public void UtcDateTimeConverter_ReadJson_Throws_WhenNull()
//     {
//         var converter = new TeamExternalService.UtcDateTimeConverter();
//         var reader = new Mock<JsonReader>();
//         reader.Setup(r => r.Value).Returns(null!);

//         Assert.Throws<JsonSerializationException>(() =>
//             converter.ReadJson(
//                 reader.Object,
//                 typeof(DateTime),
//                 default,
//                 false,
//                 new JsonSerializer()
//             )
//         );
//     }
// }
