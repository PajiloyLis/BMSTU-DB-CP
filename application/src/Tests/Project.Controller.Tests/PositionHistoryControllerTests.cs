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
using Project.Core.Models.PositionHistory;
using Project.Core.Services;
using Project.Dto.Http.PositionHistory;
using Xunit;

namespace Project.Tests.Controllers
{
    public class PositionHistoryControllerTests
    {
        private readonly Mock<IPositionHistoryService> _mockService;
        private readonly Mock<ILogger<PositionHistoryController>> _mockLogger;
        private readonly PositionHistoryController _controller;

        public PositionHistoryControllerTests()
        {
            _mockService = new Mock<IPositionHistoryService>();
            _mockLogger = new Mock<ILogger<PositionHistoryController>>();
            _controller = new PositionHistoryController(_mockLogger.Object, _mockService.Object);
        }

        #region GetPositionHistory Tests
        [Fact]
        public async Task GetPositionHistory_ValidIds_ReturnsPositionHistory()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            var testHistory = new BasePositionHistory(
                positionId,
                employeeId,
                new DateOnly(2020, 1, 1),
                new DateOnly(2022, 1, 1));

            _mockService.Setup(x => x.GetPositionHistoryAsync(positionId, employeeId))
                .ReturnsAsync(testHistory);

            // Act
            var result = await _controller.GetPositionHistory(employeeId, positionId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<PositionHistoryDto>(okResult.Value);
            Assert.Equal(positionId, dto.PositionId);
            Assert.Equal(employeeId, dto.EmployeeId);
        }

        [Fact]
        public async Task GetPositionHistory_NotFound_Returns404()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            _mockService.Setup(x => x.GetPositionHistoryAsync(positionId, employeeId))
                .ThrowsAsync(new PositionHistoryNotFoundException("Not found"));

            // Act
            var result = await _controller.GetPositionHistory(employeeId, positionId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("PositionHistoryNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region CreatePositionHistory Tests
        [Fact]
        public async Task CreatePositionHistory_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new CreatePositionHistoryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2020, 1, 1),
                new DateOnly(2022, 1, 1));

            var expectedHistory = new BasePositionHistory(
                request.PositionId,
                request.EmployeeId,
                request.StartDate,
                request.EndDate);

            _mockService.Setup(x => x.AddPositionHistoryAsync(
                request.PositionId,
                request.EmployeeId,
                request.StartDate,
                request.EndDate))
                .ReturnsAsync(expectedHistory);

            // Act
            var result = await _controller.CreatePositionHistory(request);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            var dto = Assert.IsType<PositionHistoryDto>(createdResult.Value);
            Assert.Equal(request.PositionId, dto.PositionId);
        }

        [Fact]
        public async Task CreatePositionHistory_WithoutEndDate_ReturnsCreated()
        {
            // Arrange
            var request = new CreatePositionHistoryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2020, 1, 1),
                null);

            var expectedHistory = new BasePositionHistory(
                request.PositionId,
                request.EmployeeId,
                request.StartDate,
                null);

            _mockService.Setup(x => x.AddPositionHistoryAsync(
                request.PositionId,
                request.EmployeeId,
                request.StartDate,
                request.EndDate))
                .ReturnsAsync(expectedHistory);

            // Act
            var result = await _controller.CreatePositionHistory(request);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            var dto = Assert.IsType<PositionHistoryDto>(createdResult.Value);
            Assert.Null(dto.EndDate);
        }

        [Fact]
        public async Task CreatePositionHistory_InvalidDateRange_Returns400()
        {
            // Arrange
            var invalidRequest = new CreatePositionHistoryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2022, 1, 1),
                new DateOnly(2020, 1, 1)); // End before start

            _mockService.Setup(x => x.AddPositionHistoryAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly?>()))
                .ThrowsAsync(new ArgumentException("Invalid date range"));

            // Act
            var result = await _controller.CreatePositionHistory(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }
        #endregion

        #region UpdatePositionHistory Tests
        [Fact]
        public async Task UpdatePositionHistory_FullUpdate_ReturnsOk()
        {
            // Arrange
            var request = new UpdatePositionHistoryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2021, 1, 1),
                new DateOnly(2023, 1, 1));

            var expectedHistory = new BasePositionHistory(
                request.PositionId,
                request.EmployeeId,
                request.StartDate!.Value,
                request.EndDate);

            _mockService.Setup(x => x.UpdatePositionHistoryAsync(
                request.PositionId,
                request.EmployeeId,
                request.StartDate,
                request.EndDate))
                .ReturnsAsync(expectedHistory);

            // Act
            var result = await _controller.UpdatePositionHistory(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<PositionHistoryDto>(okResult.Value);
            Assert.Equal(request.StartDate, dto.StartDate);
        }

        [Fact]
        public async Task UpdatePositionHistory_PartialUpdate_ReturnsOk()
        {
            // Arrange
            var request = new UpdatePositionHistoryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                endDate: new DateOnly(2023, 1, 1)); // Only update end date

            var existingHistory = new BasePositionHistory(
                request.PositionId,
                request.EmployeeId,
                new DateOnly(2020, 1, 1),
                new DateOnly(2022, 1, 1));

            _mockService.Setup(x => x.UpdatePositionHistoryAsync(
                request.PositionId,
                request.EmployeeId,
                request.StartDate,
                request.EndDate))
                .ReturnsAsync(new BasePositionHistory(
                    request.PositionId,
                    request.EmployeeId,
                    existingHistory.StartDate, // Keep original
                    request.EndDate)); // Updated

            // Act
            var result = await _controller.UpdatePositionHistory(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<PositionHistoryDto>(okResult.Value);
            Assert.Equal(new DateOnly(2023, 1, 1), dto.EndDate);
        }

        [Fact]
        public async Task UpdatePositionHistory_NotFound_Returns404()
        {
            // Arrange
            var request = new UpdatePositionHistoryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                endDate: new DateOnly(2023, 1, 1));

            _mockService.Setup(x => x.UpdatePositionHistoryAsync(
                request.PositionId,
                request.EmployeeId,
                request.StartDate,
                request.EndDate))
                .ThrowsAsync(new PositionHistoryNotFoundException("Not found"));

            // Act
            var result = await _controller.UpdatePositionHistory(request);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("PositionHistoryNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region DeletePositionHistory Tests
        [Fact]
        public async Task DeletePositionHistory_ValidIds_Returns204()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            _mockService.Setup(x => x.DeletePositionHistoryAsync(positionId, employeeId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeletePositionHistory(positionId, employeeId);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
        }

        [Fact]
        public async Task DeletePositionHistory_NotFound_Returns404()
        {
            // Arrange
            var positionId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();
            _mockService.Setup(x => x.DeletePositionHistoryAsync(positionId, employeeId))
                .ThrowsAsync(new PositionHistoryNotFoundException("Not found"));

            // Act
            var result = await _controller.DeletePositionHistory(positionId, employeeId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("PositionHistoryNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region GetPositionHistorysByEmployeeId Tests
        [Fact]
        public async Task GetPositionHistorysByEmployeeId_ValidRequest_ReturnsPagedList()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var histories = new List<BasePositionHistory>
            {
                new BasePositionHistory(Guid.NewGuid(), employeeId, new DateOnly(2020, 1, 1), new DateOnly(2021, 1, 1)),
                new BasePositionHistory(Guid.NewGuid(), employeeId, new DateOnly(2021, 1, 1), null)
            };

            var page = new Page(1, 2, 10);
            var pagedResult = new PositionHistoryPage(histories, page);

            _mockService.Setup(x => x.GetPositionHistoryByEmployeeIdAsync(employeeId, 1, 10, null, null))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetPositionHistorysByEmployeeId(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<PositionHistoryDto>>(okResult.Value);
            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public async Task GetPositionHistorysByEmployeeId_WithDateFilter_ReturnsFiltered()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var startDate = new DateOnly(2021, 1, 1);
            var endDate = new DateOnly(2022, 1, 1);

            var histories = new List<BasePositionHistory>
            {
                new BasePositionHistory(Guid.NewGuid(), employeeId, new DateOnly(2021, 6, 1), new DateOnly(2021, 12, 1))
            };

            var page = new Page(1, 1, 10);
            var pagedResult = new PositionHistoryPage(histories, page);

            _mockService.Setup(x => x.GetPositionHistoryByEmployeeIdAsync(employeeId, 1, 10, startDate, endDate))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetPositionHistorysByEmployeeId(employeeId, startDate: startDate, endDate: endDate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<PositionHistoryDto>>(okResult.Value);
            Assert.Single(dtos);
        }
        #endregion

        #region GetSubordinatesPositionHistoriesByHeadEmployeeId Tests
        [Fact]
        public async Task GetSubordinatesPositionHistoriesByHeadEmployeeId_ValidRequest_ReturnsPagedList()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var histories = new List<BasePositionHistory>
            {
                new BasePositionHistory(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2020, 1, 1), null)
            };

            var page = new Page(1, 1, 10);
            var pagedResult = new PositionHistoryPage(histories, page);

            _mockService.Setup(x => x.GetCurrentSubordinatesPositionHistoryAsync(employeeId, 1, 10, null, null))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetSubordinatesPositionHistoriesByHeadEmployeeId(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<PositionHistoryDto>>(okResult.Value);
            Assert.Single(dtos);
        }
        #endregion

        #region GetCurrentSubordinatesByHeadEmployeeId Tests
        [Fact]
        public async Task GetCurrentSubordinatesByHeadEmployeeId_ValidRequest_ReturnsPagedList()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var subordinates = new List<PositionHierarchyWithEmployee>
            {
                new PositionHierarchyWithEmployee(Guid.NewGuid(), Guid.NewGuid(), null, "Subordinate", 1)
            };

            var page = new Page(1, 1, 10);
            var pagedResult = new PositionHierarchyWithEmployeePage(subordinates, page);

            _mockService.Setup(x => x.GetCurrentSubordinatesAsync(employeeId, 1, 10))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetCurrentSubordinatesByHeadEmployeeId(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<IEnumerable<PositionHierarchyWithEmployeeDto>>(okResult.Value);
            Assert.Single(dtos);
        }
        #endregion

        #region Edge Cases
        [Fact]
        public async Task CreatePositionHistory_FutureStartDate_Returns400()
        {
            // Arrange
            var invalidRequest = new CreatePositionHistoryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.Now.AddDays(1)), // Future date
                null);

            _mockService.Setup(x => x.AddPositionHistoryAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly?>()))
                .ThrowsAsync(new ArgumentException("Start date must be in the past"));

            // Act
            var result = await _controller.CreatePositionHistory(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }

        [Fact]
        public async Task UpdatePositionHistory_InvalidEmployeeId_Returns400()
        {
            // Arrange
            var invalidRequest = new UpdatePositionHistoryDto(
                Guid.NewGuid(),
                Guid.Empty, // Invalid
                endDate: new DateOnly(2023, 1, 1));

            _mockService.Setup(x => x.UpdatePositionHistoryAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<DateOnly?>()))
                .ThrowsAsync(new ArgumentException("Invalid Employee ID"));

            // Act
            var result = await _controller.UpdatePositionHistory(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }
        #endregion
    }
}