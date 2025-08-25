using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Teams.API.Layer.Controllers;
using Teams.API.Layer.DTOs;
using Teams.APP.Layer.CQRS.Commands;
using Teams.APP.Layer.CQRS.Queries;
using Teams.APP.Layer.Interfaces;
using Xunit;

namespace Teams.Tests
{
    public class TeamControllerTest
    {
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<IValidator<CreateTeamCommand>> _createTeamValidatorMock = new();
        private readonly Mock<IValidator<UpdateTeamCommand>> _updateTeamValidatorMock = new();
        private readonly Mock<
            IValidator<UpdateTeamManagerCommand>
        > _updateTeamManagerValidatorMock = new();
        private readonly Mock<IEmployeeService> _employeeServiceMock = new();

        private TeamController CreateController()
        {
            return new TeamController(
                _mediatorMock.Object,
                _createTeamValidatorMock.Object,
                _updateTeamValidatorMock.Object,
                _updateTeamManagerValidatorMock.Object,
                _employeeServiceMock.Object
            );
        }

        [Fact]
        public async Task GetAllTeams_ReturnsOkResult()
        {
            var teams = new List<TeamDto> { new TeamDto() };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllTeamsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(teams);

            var controller = CreateController();
            var result = await controller.GetAllTeams();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(teams, okResult.Value);
        }

        [Fact]
        public async Task GetTeam_ReturnsOkResult()
        {
            var teamId = Guid.NewGuid();
            var team = new TeamDto { Id = teamId };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetTeamQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(team);

            var controller = CreateController();
            var result = await controller.GetTeam(teamId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(team, okResult.Value);
        }

        [Fact]
        public async Task GetTeamsByManagerId_ReturnsOkResult()
        {
            var managerId = Guid.NewGuid();
            var teams = new List<TeamRequestDto> { new TeamRequestDto() };
            _mediatorMock
                .Setup(m =>
                    m.Send(It.IsAny<GetTeamsByManagerQuery>(), It.IsAny<CancellationToken>())
                )
                .Returns(Task.FromResult(teams));

            var controller = CreateController();
            var result = await controller.GetTeamsByManagerId(managerId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(teams, okResult.Value);
        }

        [Fact]
        public async Task ChangeTeamManager_InvalidModel_ReturnsBadRequest()
        {
            var command = new UpdateTeamManagerCommand(
                "TeamName",
                Guid.NewGuid(),
                Guid.NewGuid(),
                "ManagerName"
            );
            var validationResult = new ValidationResult(
                new[] { new ValidationFailure("Name", "Required") }
            );
            _updateTeamManagerValidatorMock
                .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var controller = CreateController();
            var result = await controller.ChangeTeamManager(command);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ChangeTeamManager_ValidModel_ReturnsNoContent()
        {
            var command = new UpdateTeamManagerCommand(
                "TeamName",
                Guid.NewGuid(),
                Guid.NewGuid(),
                "ManagerName"
            );
            var validationResult = new ValidationResult();
            _updateTeamManagerValidatorMock
                .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Unit.Value));

            var controller = CreateController();
            var result = await controller.ChangeTeamManager(command);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task GetTeamsByMemberId_ReturnsOkResult()
        {
            var memberId = Guid.NewGuid();
            var teams = new List<TeamRequestDto> { new TeamRequestDto() };
            _mediatorMock
                .Setup(m =>
                    m.Send(It.IsAny<GetTeamsByMemberQuery>(), It.IsAny<CancellationToken>())
                )
                .Returns(Task.FromResult(teams));

            var controller = CreateController();
            var result = await controller.GetTeamsByMemberId(memberId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(teams, okResult.Value);
        }

        [Fact]
        public async Task AddTeamMember_EmptyMemberId_ReturnsBadRequest()
        {
            var controller = CreateController();
            var result = await controller.AddTeamMember(Guid.Empty);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Member ID cannot be empty.", badRequest.Value);
        }

        [Fact]
        public async Task AddTeamMember_NotFound_ReturnsNotFound()
        {
            var memberId = Guid.NewGuid();
            _employeeServiceMock
                .Setup(e =>
                    e.InsertNewTeamMemberIntoDbAsync(memberId, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(false);

            var controller = CreateController();
            var result = await controller.AddTeamMember(memberId);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
            Assert.Contains(memberId.ToString(), notFound.Value!.ToString());
        }

        [Fact]
        public async Task AddTeamMember_Success_ReturnsNoContent()
        {
            var memberId = Guid.NewGuid();
            _employeeServiceMock
                .Setup(e =>
                    e.InsertNewTeamMemberIntoDbAsync(memberId, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(true);

            var controller = CreateController();
            var result = await controller.AddTeamMember(memberId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CreateTeam_InvalidModel_ReturnsBadRequest()
        {
            var command = new CreateTeamCommand();
            var validationResult = new ValidationResult(
                new[] { new ValidationFailure("Name", "Required") }
            );
            _createTeamValidatorMock
                .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var controller = CreateController();
            var result = await controller.CreateTeam(command);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateTeam_ValidModel_ReturnsCreatedAtAction()
        {
            var command = new CreateTeamCommand();
            var teamDto = new TeamDto { Id = Guid.NewGuid() };
            var validationResult = new ValidationResult();
            _createTeamValidatorMock
                .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(teamDto));

            var controller = CreateController();
            var result = await controller.CreateTeam(command);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(teamDto, createdResult.Value);
        }

        [Fact]
        public async Task UpdateTeamById_IdMismatch_ReturnsBadRequest()
        {
            var command = new UpdateTeamCommand { Id = Guid.NewGuid() };
            var controller = CreateController();
            var result = await controller.UpdateTeamById(Guid.NewGuid(), command);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(
                "Team ID in the URL does not match the ID in the request body.",
                badRequest.Value
            );
        }

        [Fact]
        public async Task UpdateTeamById_InvalidModel_ReturnsBadRequest()
        {
            var teamId = Guid.NewGuid();
            var command = new UpdateTeamCommand { Id = teamId };
            var validationResult = new ValidationResult(
                new[] { new ValidationFailure("Name", "Required") }
            );
            _updateTeamValidatorMock
                .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            var controller = CreateController();
            var result = await controller.UpdateTeamById(teamId, command);

            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateTeamById_ValidModel_ReturnsOk()
        {
            var teamId = Guid.NewGuid();
            var command = new UpdateTeamCommand { Id = teamId };
            var teamRequestDto = new TeamRequestDto { Id = teamId };
            var validationResult = new ValidationResult();
            _updateTeamValidatorMock
                .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(teamRequestDto));

            var controller = CreateController();
            var result = await controller.UpdateTeamById(teamId, command);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(teamRequestDto, okResult.Value);
        }

        [Fact]
        public async Task DeleteTeamMemberById_NullDto_ReturnsBadRequest()
        {
            var controller = CreateController();
            var result = await controller.DeleteTeamMemberById(null!);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Request data cannot be null.", badRequest.Value);
        }

        [Fact]
        public async Task DeleteTeamMemberById_EmptyTeamName_ReturnsBadRequest()
        {
            var dto = new DeleteTeamMemberDto(Guid.NewGuid(), "");
            var controller = CreateController();
            var result = await controller.DeleteTeamMemberById(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Team name must be provided.", badRequest.Value);
        }

        [Fact]
        public async Task DeleteTeamMemberById_Valid_ReturnsNoContent()
        {
            var dto = new DeleteTeamMemberDto(Guid.NewGuid(), "Team");
            _employeeServiceMock
                .Setup(e =>
                    e.DeleteTeamMemberAsync(
                        dto.MemberId,
                        dto.TeamName!,
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var controller = CreateController();
            var result = await controller.DeleteTeamMemberById(dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTeam_ReturnsNoContent()
        {
            var teamId = Guid.NewGuid();
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteTeamCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController();
            var result = await controller.DeleteTeam(teamId);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
