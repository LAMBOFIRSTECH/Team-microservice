// using System;
// using AutoMapper;
// using Teams.API.Layer.DTOs;
// using Teams.API.Layer.Mappings;
// using Teams.CORE.Layer.Entities;
// using Xunit;

// namespace Teams.Tests.API
// {
//     public class TeamProfileTest
//     {
//         private readonly IMapper _mapper;

//         public TeamProfileTest()
//         {
//             var configuration = new MapperConfiguration(cfg =>
//             {
//                 cfg.AddProfile<TeamProfile>();
//             });

//             configuration.AssertConfigurationIsValid();
//             _mapper = configuration.CreateMapper();
//         }

//         [Fact]
//         public void Should_Map_Team_To_TeamDto()
//         {
//             var team = new Team
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Team Alpha",
//                 TeamManagerId = Guid.NewGuid(),
//                 MembersId = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
//             };

//             var dto = _mapper.Map<TeamDto>(team);

//             Assert.Equal(team.Id, dto.Id);
//             Assert.Equal(team.Name, dto.Name);
//             Assert.Equal(team.TeamManagerId, dto.TeamManagerId);
//             Assert.Equal(team.MembersId.Count, dto.MembersId.Count);
//         }

//         [Fact]
//         public void Should_Map_Team_To_TeamRequestDto()
//         {
//             var team = new Team
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Team Beta",
//                 TeamManagerId = Guid.NewGuid(),
//                 MembersId = new List<Guid> { Guid.NewGuid() },
//             };

//             var dto = _mapper.Map<TeamRequestDto>(team);

//             Assert.Equal(team.Id, dto.Id);
//             Assert.Equal(team.Name, dto.Name);
//             Assert.Equal(team.TeamManagerId, dto.TeamManagerId);
//             Assert.Equal(team.MembersId.Count, dto.MemberId.Count);
//         }

//         [Fact]
//         public void Should_Map_Team_To_ChangeManagerDto()
//         {
//             var oldManagerId = Guid.NewGuid();
//             var newManagerId = Guid.NewGuid();
//             var team = new Team
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Team Gamma",
//                 TeamManagerId = oldManagerId,
//             };

//             var dto = _mapper.Map<ChangeManagerDto>(team);

//             Assert.Equal(team.Name, dto.Name);
//             Assert.Equal(team.TeamManagerId, dto.OldTeamManagerId); // selon convention, tu peux adapter
//             Assert.Equal(Guid.Empty, dto.NewTeamManagerId); // AutoMapper ne connait pas la cible NewTeamManagerId
//         }
//     }
// }
