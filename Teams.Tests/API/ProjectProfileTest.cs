// using System;
// using System.Collections.Generic;
// using AutoMapper;
// using Teams.API.Layer.Mappings;
// using Teams.CORE.Layer.ValueObjects;
// using Teams.INFRA.Layer.ExternalServicesDtos;
// using Xunit;

// namespace Teams.Tests.API
// {
//     public class ProjectProfileTest
//     {
//         private readonly IMapper _mapper;

//         public ProjectProfileTest()
//         {
//             var configuration = new MapperConfiguration(cfg =>
//             {
//                 cfg.AddProfile<ProjectProfile>();
//             });

//             configuration.AssertConfigurationIsValid(); // Vérifie la configuration AutoMapper
//             _mapper = configuration.CreateMapper();
//         }

//         [Fact]
//         public void Should_Map_ProjectStateDto_To_ProjectState()
//         {
//             var dto = new ProjectStateDto { State = ProjectState.Suspended };

//             var result = _mapper.Map<ProjectState>(dto);

//             Assert.Equal(ProjectState.Suspended, result);
//         }

//         [Fact]
//         public void Should_Map_DetailDto_To_Detail()
//         {
//             var dto = new DetailDto
//             {
//                 ProjectName = "Project X",
//                 ProjectStartDate = new DateTime(2025, 1, 1),
//                 ProjectEndDate = new DateTime(2025, 12, 31),
//                 ProjectState = new ProjectStateDto { State = ProjectState.Active },
//             };

//             var result = _mapper.Map<Detail>(dto);

//             Assert.Equal(dto.ProjectName, result.ProjectName);
//             Assert.Equal(dto.ProjectStartDate, result.ProjectStartDate);
//             Assert.Equal(dto.ProjectEndDate, result.ProjectEndDate);
//             Assert.Equal(ProjectState.Active, result.State);
//         }

//         [Fact]
//         public void Should_Map_ProjectAssociationDto_To_ProjectAssociation()
//         {
//             var dto = new ProjectAssociationDto
//             {
//                 TeamManagerId = Guid.NewGuid(),
//                 TeamName = "Team Alpha",
//                 Details = new List<DetailDto>
//                 {
//                     new DetailDto
//                     {
//                         ProjectName = "Project 1",
//                         ProjectStartDate = DateTime.Today,
//                         ProjectEndDate = DateTime.Today.AddDays(10),
//                         ProjectState = new ProjectStateDto { State = ProjectState.Active },
//                     },
//                     new DetailDto
//                     {
//                         ProjectName = "Project 2",
//                         ProjectStartDate = DateTime.Today.AddDays(1),
//                         ProjectEndDate = DateTime.Today.AddDays(11),
//                         ProjectState = new ProjectStateDto { State = ProjectState.Suspended },
//                     },
//                 },
//             };

//             var result = _mapper.Map<ProjectAssociation>(dto);

//             Assert.Equal(dto.TeamManagerId, result.TeamManagerId);
//             Assert.Equal(dto.TeamName, result.TeamName);
//             Assert.Equal(dto.Details.Count, result.Details.Count);
//             Assert.Equal(dto.Details[0].ProjectName, result.Details[0].ProjectName);
//             Assert.Equal(dto.Details[1].State, result.Details[1].State);
//         }
//     }
// }
