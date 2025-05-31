using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Dto.Http;
using Project.HttpServer.Controllers;
using Project.Core.Models;
using Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Project.Core.Exceptions;
using Project.Core.Models.Education;
using Project.Core.Services;
using Project.Dto.Http.Education;
using Xunit;

namespace Project.Tests.Controllers
{
    public class EducationControllerTests
    {
        private readonly Mock<IEducationService> _mockService;
        private readonly Mock<ILogger<EducationController>> _mockLogger;
        private readonly EducationController _controller;

        public EducationControllerTests()
        {
            _mockService = new Mock<IEducationService>();
            _mockLogger = new Mock<ILogger<EducationController>>();
            _controller = new EducationController(_mockLogger.Object, _mockService.Object);
        }

        #region GetEducation Tests
        [Fact]
        public async Task GetEducation_ValidId_ReturnsEducation()
        {
            // Arrange
            var educationId = Guid.NewGuid();
            var testEducation = new BaseEducation(
                educationId,
                Guid.NewGuid(),
                "MIT",
                "Высшее (бакалавриат)",
                "Computer Science",
                new DateOnly(2010, 9, 1),
                new DateOnly(2014, 6, 30));

            _mockService.Setup(x => x.GetEducationByIdAsync(educationId))
                .ReturnsAsync(testEducation);

            // Act
            var result = await _controller.GetEducation(educationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<EducationDto>(okResult.Value);
            Assert.Equal(educationId, dto.Id);
            Assert.Equal("MIT", dto.Institution);
        }

        [Fact]
        public async Task GetEducation_NotFound_Returns404()
        {
            // Arrange
            var educationId = Guid.NewGuid();
            _mockService.Setup(x => x.GetEducationByIdAsync(educationId))
                .ThrowsAsync(new EducationNotFoundException("Not found"));

            // Act
            var result = await _controller.GetEducation(educationId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("EducationNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region CreateEducation Tests
        [Fact]
        public async Task CreateEducation_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new CreateEducationDto(
                Guid.NewGuid(),
                "Harvard University",
                "Высшее (магистратура)",
                "Business Administration",
                new DateOnly(2015, 9, 1),
                new DateOnly(2017, 6, 30));

            var expectedEducation = new BaseEducation(
                Guid.NewGuid(),
                request.EmployeeId,
                request.Institution,
                request.Level,
                request.StudyField,
                request.StartDate,
                request.EndDate);

            _mockService.Setup(x => x.AddEducationAsync(
                request.EmployeeId,
                request.Institution,
                request.Level,
                request.StudyField,
                request.StartDate,
                request.EndDate))
                .ReturnsAsync(expectedEducation);

            // Act
            var result = await _controller.CreateEducation(request);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            var dto = Assert.IsType<EducationDto>(createdResult.Value);
            Assert.Equal(expectedEducation.Id, dto.Id);
        }

        [Fact]
        public async Task CreateEducation_Duplicate_Returns400()
        {
            // Arrange
            var request = new CreateEducationDto(
                Guid.NewGuid(),
                "Stanford",
                "PhD",
                "Computer Science",
                new DateOnly(2018, 9, 1),
                null);

            _mockService.Setup(x => x.AddEducationAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly?>()))
                .ThrowsAsync(new EducationAlreadyExistsException("Duplicate"));

            // Act
            var result = await _controller.CreateEducation(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("EducationAlreadyExistsException", errorDto.ErrorType);
        }

        [Fact]
        public async Task CreateEducation_InvalidData_Returns400()
        {
            // Arrange
            var invalidRequest = new CreateEducationDto(
                Guid.Empty, // Invalid
                "", // Invalid
                "Bachelor",
                "CS",
                new DateOnly(2020, 9, 1),
                null);

            _mockService.Setup(x => x.AddEducationAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly?>()))
                .ThrowsAsync(new ArgumentException("Invalid data"));

            // Act
            var result = await _controller.CreateEducation(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }
        #endregion

        #region UpdateEducation Tests
        [Fact]
        public async Task UpdateEducation_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new UpdateEducationDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Updated University",
                "Высшее (магистратура)",
                "Updated Field",
                new DateOnly(2015, 9, 1),
                new DateOnly(2017, 6, 30));

            var expectedEducation = new BaseEducation(
                request.Id,
                request.EmployeeId,
                request.Institution!,
                request.Level!,
                request.StudyField!,
                request.StartDate!.Value,
                request.EndDate);

            _mockService.Setup(x => x.UpdateEducationAsync(
                request.Id,
                request.EmployeeId,
                request.Institution,
                request.Level,
                request.StudyField,
                request.StartDate,
                request.EndDate))
                .ReturnsAsync(expectedEducation);

            // Act
            var result = await _controller.UpdateEducation(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<EducationDto>(okResult.Value);
            Assert.Equal("Updated University", dto.Institution);
        }

        [Fact]
        public async Task UpdateEducation_PartialUpdate_ReturnsOk()
        {
            // Arrange
            var request = new UpdateEducationDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                "Высшее (магистратура)",
                null,
                null,
                null);

            var existingEducation = new BaseEducation(
                request.Id,
                request.EmployeeId,
                "Original University",
                "Высшее (бакалавриат)",
                "Original Field",
                new DateOnly(2010, 9, 1),
                new DateOnly(2014, 6, 30));

            _mockService.Setup(x => x.UpdateEducationAsync(
                request.Id,
                request.EmployeeId,
                request.Institution,
                request.Level,
                request.StudyField,
                request.StartDate,
                request.EndDate))
                .ReturnsAsync(new BaseEducation(
                    request.Id,
                    request.EmployeeId,
                    existingEducation.Institution, // Keep original
                    request.Level!, // Updated
                    existingEducation.StudyField, // Keep original
                    existingEducation.StartDate, // Keep original
                    existingEducation.EndDate)); // Keep original

            // Act
            var result = await _controller.UpdateEducation(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<EducationDto>(okResult.Value);
            Assert.Equal("Высшее (магистратура)", dto.Level);
            Assert.Equal("Original University", dto.Institution);
        }
        #endregion

        #region DeleteEducation Tests
        [Fact]
        public async Task DeleteEducation_ValidId_Returns204()
        {
            // Arrange
            var educationId = Guid.NewGuid();
            _mockService.Setup(x => x.DeleteEducationAsync(educationId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteEducation(educationId);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
        }

        [Fact]
        public async Task DeleteEducation_NotFound_Returns404()
        {
            // Arrange
            var educationId = Guid.NewGuid();
            _mockService.Setup(x => x.DeleteEducationAsync(educationId))
                .ThrowsAsync(new EducationNotFoundException("Not found"));

            // Act
            var result = await _controller.DeleteEducation(educationId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("EducationNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region GetEducationsByEmployeeId Tests
        [Fact]
        public async Task GetEducationsByEmployeeId_ValidRequest_ReturnsPagedList()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var educations = new List<BaseEducation>
            {
                new BaseEducation(
                    Guid.NewGuid(),
                    employeeId,
                    "University 1",
                    "Высшее (бакалавриат)",
                    "Field 1",
                    new DateOnly(2010, 9, 1),
                    new DateOnly(2014, 6, 30)),
                new BaseEducation(
                    Guid.NewGuid(),
                    employeeId,
                    "University 2",
                    "Высшее (магистратура)",
                    "Field 2",
                    new DateOnly(2015, 9, 1),
                    new DateOnly(2017, 6, 30))
            };

            var page = new Page(1, 2, 10);
            var pagedResult = new EducationPage(educations, page);

            _mockService.Setup(x => x.GetEducationsByEmployeeIdAsync(employeeId, 1, 10))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetEducationsByEmployeeId(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<EducationDto>>(okResult.Value);
            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public async Task GetEducationsByEmployeeId_CustomPageSize_ReturnsCorrectPage()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var educations = new List<BaseEducation>
            {
                new BaseEducation(
                    Guid.NewGuid(),
                    employeeId,
                    "University",
                    "Высшее (бакалавриат)",
                    "Field",
                    new DateOnly(2010, 9, 1),
                    new DateOnly(2014, 6, 30))
            };

            var page = new Page(2, 1, 1);
            var pagedResult = new EducationPage(educations, page);

            _mockService.Setup(x => x.GetEducationsByEmployeeIdAsync(employeeId, 2, 1))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetEducationsByEmployeeId(employeeId, pageNumber: 2, pageSize: 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<EducationDto>>(okResult.Value);
            Assert.Single(dtos);
        }
        #endregion

        #region Edge Cases
        [Fact]
        public async Task CreateEducation_InvalidDateRange_Returns400()
        {
            // Arrange
            var invalidRequest = new CreateEducationDto(
                Guid.NewGuid(),
                "University",
                "Bachelor",
                "CS",
                new DateOnly(2020, 9, 1),
                new DateOnly(2019, 6, 30)); // EndDate before StartDate

            _mockService.Setup(x => x.AddEducationAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly?>()))
                .ThrowsAsync(new ArgumentException("Invalid date range"));

            // Act
            var result = await _controller.CreateEducation(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }

        [Fact]
        public async Task UpdateEducation_InvalidLevel_Returns400()
        {
            // Arrange
            var invalidRequest = new UpdateEducationDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                null,
                "Invalid Level", // Invalid
                null,
                null,
                null);

            _mockService.Setup(x => x.UpdateEducationAsync(
                invalidRequest.Id,
                invalidRequest.EmployeeId,
                invalidRequest.Institution,
                invalidRequest.Level,
                invalidRequest.StudyField,
                invalidRequest.StartDate,
                invalidRequest.EndDate))
                .ThrowsAsync(new ArgumentException("Invalid level"));

            // Act
            var result = await _controller.UpdateEducation(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }
        #endregion
    }
}