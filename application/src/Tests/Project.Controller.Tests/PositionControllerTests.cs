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
using Project.Core.Models.Position;
using Project.Core.Services;
using Project.Dto.Http.Position;
using Xunit;

namespace Project.Tests.Controllers
{
    public class PositionControllerTests
    {
        private readonly Mock<IPositionService> _mockService;
        private readonly Mock<ILogger<PositionController>> _mockLogger;
        private readonly PositionController _controller;

        public PositionControllerTests()
        {
            _mockService = new Mock<IPositionService>();
            _mockLogger = new Mock<ILogger<PositionController>>();
            _controller = new PositionController(_mockLogger.Object, _mockService.Object);
        }

        #region GetPosition Tests
        [Fact]
        public async Task GetPosition_ValidId_ReturnsPosition()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            var testPosition = new BasePosition(
                positionId,
                Guid.NewGuid(),
                "Software Engineer",
                Guid.NewGuid());

            _mockService.Setup(x => x.GetPositionByIdAsync(positionId))
                .ReturnsAsync(testPosition);

            // Act
            var result = await _controller.GetPosition(positionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<PositionDto>(okResult.Value);
            Assert.Equal(positionId, dto.Id);
            Assert.Equal("Software Engineer", dto.Title);
        }

        [Fact]
        public async Task GetPosition_NotFound_Returns404()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            _mockService.Setup(x => x.GetPositionByIdAsync(positionId))
                .ThrowsAsync(new PositionNotFoundException("Not found"));

            // Act
            var result = await _controller.GetPosition(positionId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("PositionNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region CreatePosition Tests
        [Fact]
        public async Task CreatePosition_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new CreatePositionDto(
                Guid.NewGuid(), // ParentId
                "Senior Developer",
                Guid.NewGuid()); // CompanyId

            var expectedPosition = new BasePosition(
                Guid.NewGuid(),
                request.ParentId.Value,
                request.Title,
                request.CompanyId);

            _mockService.Setup(x => x.AddPositionAsync(
                request.ParentId,
                request.Title,
                request.CompanyId))
                .ReturnsAsync(expectedPosition);

            // Act
            var result = await _controller.CreatePosition(request);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            var dto = Assert.IsType<PositionDto>(createdResult.Value);
            Assert.Equal(expectedPosition.Id, dto.Id);
        }

        [Fact]
        public async Task CreatePosition_WithoutParent_ReturnsCreated()
        {
            // Arrange
            var request = new CreatePositionDto(
                null, // No parent
                "CEO",
                Guid.NewGuid());

            var expectedPosition = new BasePosition(
                Guid.NewGuid(),
                Guid.Empty, // Default parent
                request.Title,
                request.CompanyId);

            _mockService.Setup(x => x.AddPositionAsync(
                request.ParentId,
                request.Title,
                request.CompanyId))
                .ReturnsAsync(expectedPosition);

            // Act
            var result = await _controller.CreatePosition(request);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            var dto = Assert.IsType<PositionDto>(createdResult.Value);
            Assert.Equal("CEO", dto.Title);
        }

        [Fact]
        public async Task CreatePosition_Duplicate_Returns400()
        {
            // Arrange
            var request = new CreatePositionDto(
                Guid.NewGuid(),
                "Duplicate Title",
                Guid.NewGuid());

            _mockService.Setup(x => x.AddPositionAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()))
                .ThrowsAsync(new PositionAlreadyExistsException("Duplicate"));

            // Act
            var result = await _controller.CreatePosition(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("PositionAlreadyExistsException", errorDto.ErrorType);
        }

        [Fact]
        public async Task CreatePosition_InvalidData_Returns400()
        {
            // Arrange
            var invalidRequest = new CreatePositionDto(
                null,
                "", // Invalid title
                Guid.NewGuid());

            _mockService.Setup(x => x.AddPositionAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()))
                .ThrowsAsync(new ArgumentException("Invalid data"));

            // Act
            var result = await _controller.CreatePosition(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }
        #endregion

        #region UpdatePosition Tests
        [Fact]
        public async Task UpdatePosition_FullUpdate_ReturnsOk()
        {
            // Arrange
            var request = new UpdatePositionDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(), // New parent
                "Updated Title");

            var expectedPosition = new BasePosition(
                request.Id,
                request.ParentId.Value,
                request.Title!,
                request.CompanyId);

            _mockService.Setup(x => x.UpdatePositionAsync(
                request.Id,
                request.CompanyId,
                request.ParentId,
                request.Title))
                .ReturnsAsync(expectedPosition);

            // Act
            var result = await _controller.UpdatePosition(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<PositionDto>(okResult.Value);
            Assert.Equal("Updated Title", dto.Title);
        }

        [Fact]
        public async Task UpdatePosition_PartialUpdate_ReturnsOk()
        {
            // Arrange
            var request = new UpdatePositionDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                title: "New Title"); // Only update title

            var existingPosition = new BasePosition(
                request.Id,
                Guid.NewGuid(), // Original parent
                "Old Title",
                request.CompanyId);

            _mockService.Setup(x => x.UpdatePositionAsync(
                request.Id,
                request.CompanyId,
                request.ParentId,
                request.Title))
                .ReturnsAsync(new BasePosition(
                    request.Id,
                    existingPosition.ParentId, // Keep original
                    request.Title!, // Updated
                    request.CompanyId));

            // Act
            var result = await _controller.UpdatePosition(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<PositionDto>(okResult.Value);
            Assert.Equal("New Title", dto.Title);
            Assert.NotEqual(Guid.Empty, dto.ParentId);
        }

        [Fact]
        public async Task UpdatePosition_NotFound_Returns404()
        {
            // Arrange
            var request = new UpdatePositionDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                title: "New Title");

            _mockService.Setup(x => x.UpdatePositionAsync(
                request.Id,
                request.CompanyId,
                request.ParentId,
                request.Title))
                .ThrowsAsync(new PositionNotFoundException("Not found"));

            // Act
            var result = await _controller.UpdatePosition(request);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("PositionNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region DeletePosition Tests
        [Fact]
        public async Task DeletePosition_ValidId_Returns204()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            _mockService.Setup(x => x.DeletePositionAsync(positionId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeletePosition(positionId);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
        }

        [Fact]
        public async Task DeletePosition_NotFound_Returns404()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            _mockService.Setup(x => x.DeletePositionAsync(positionId))
                .ThrowsAsync(new PositionNotFoundException("Not found"));

            // Act
            var result = await _controller.DeletePosition(positionId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("PositionNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region GetSubordinatesPositionsByHeadPositionId Tests
        [Fact]
        public async Task GetSubordinatesPositionsByHeadPositionId_ValidRequest_ReturnsPagedList()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            var subordinates = new List<PositionHierarchy>
            {
                new PositionHierarchy(Guid.NewGuid(), positionId, "Subordinate 1", 1),
                new PositionHierarchy(Guid.NewGuid(), positionId, "Subordinate 2", 1)
            };

            var page = new Page(1, 2, 10);
            var pagedResult = new PositionHierarchyPage(subordinates, page);

            _mockService.Setup(x => x.GetSubordinatesAsync(positionId, 1, 10))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetSubordinatesPositionsByHeadPositionId(positionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<PositionHierarchyDto>>(okResult.Value);
            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public async Task GetSubordinatesPositionsByHeadPositionId_CustomPageSize_ReturnsCorrectPage()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            var subordinates = new List<PositionHierarchy>
            {
                new PositionHierarchy(Guid.NewGuid(), positionId, "Subordinate", 1)
            };

            var page = new Page(2, 1, 1);
            var pagedResult = new PositionHierarchyPage(subordinates, page);

            _mockService.Setup(x => x.GetSubordinatesAsync(positionId, 2, 1))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetSubordinatesPositionsByHeadPositionId(positionId, pageNumber: 2, pageSize: 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<PositionHierarchyDto>>(okResult.Value);
            Assert.Single(dtos);
        }
        #endregion

        #region Edge Cases
        [Fact]
        public async Task CreatePosition_InvalidCompanyId_Returns400()
        {
            // Arrange
            var invalidRequest = new CreatePositionDto(
                null,
                "Title",
                Guid.Empty); // Invalid CompanyId

            _mockService.Setup(x => x.AddPositionAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()))
                .ThrowsAsync(new ArgumentException("Invalid CompanyId"));

            // Act
            var result = await _controller.CreatePosition(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }

        [Fact]
        public async Task UpdatePosition_CircularReference_Returns400()
        {
            // Arrange
            var request = new UpdatePositionDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                parentId: Guid.NewGuid());

            _mockService.Setup(x => x.UpdatePositionAsync(
                request.Id,
                request.CompanyId,
                request.ParentId,
                request.Title))
                .ThrowsAsync(new ArgumentException("Circular reference detected"));

            // Act
            var result = await _controller.UpdatePosition(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }
        #endregion
    }
}