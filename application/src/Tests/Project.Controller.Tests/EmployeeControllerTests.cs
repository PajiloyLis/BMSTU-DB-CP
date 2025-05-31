using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Dto.Http;
using Project.HttpServer.Controllers;
using Project.Core.Models;
using Project.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Project.Core.Exceptions;
using Project.Core.Models.Employee;
using Project.Core.Services;
using Project.Dto.Http.Employee;
using Xunit;

namespace Project.Tests.Controllers
{
    public class EmployeeControllerTests
    {
        private readonly Mock<IEmployeeService> _mockService;
        private readonly Mock<ILogger<EmployeeController>> _mockLogger;
        private readonly EmployeeController _controller;

        public EmployeeControllerTests()
        {
            _mockService = new Mock<IEmployeeService>();
            _mockLogger = new Mock<ILogger<EmployeeController>>();
            _controller = new EmployeeController(_mockLogger.Object, _mockService.Object);
        }

        #region GetEmployee Tests
        [Fact]
        public async Task GetEmployee_ValidId_ReturnsEmployee()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var testEmployee = new BaseEmployee(
                employeeId,
                "John Doe",
                "+1234567890",
                "john@example.com",
                new DateOnly(1990, 1, 1),
                "photo.jpg",
                JsonSerializer.Serialize(new { Duty = "Manager" }));

            _mockService.Setup(x => x.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(testEmployee);

            // Act
            var result = await _controller.GetEmployee(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<EmployeeDto>(okResult.Value);
            Assert.Equal(employeeId, dto.EmployeeId);
            Assert.Equal("John Doe", dto.FullName);
        }

        [Fact]
        public async Task GetEmployee_NotFound_Returns404()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockService.Setup(x => x.GetEmployeeByIdAsync(employeeId))
                .ThrowsAsync(new EmployeeNotFoundException("Not found"));

            // Act
            var result = await _controller.GetEmployee(employeeId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("EmployeeNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region CreateEmployee Tests
        [Fact]
        public async Task CreateEmployee_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new CreateEmployeeDto(
                "John Doe",
                "+1234567890",
                "john@example.com",
                new DateOnly(1990, 1, 1),
                "photo.jpg",
                JsonSerializer.Serialize(new { Duty = "Manager" }));

            var expectedEmployee = new BaseEmployee(
                Guid.NewGuid(),
                request.FullName,
                request.PhoneNumber,
                request.Email,
                request.Birthday,
                request.PhotoPath,
                request.Duties);

            _mockService.Setup(x => x.AddEmployeeAsync(
                request.FullName,
                request.PhoneNumber,
                request.Email,
                request.Birthday,
                request.PhotoPath,
                request.Duties))
                .ReturnsAsync(expectedEmployee);

            // Act
            var result = await _controller.CreateEmployee(request);

            // Assert
            var createdResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            var dto = Assert.IsType<EmployeeDto>(createdResult.Value);
            Assert.Equal(expectedEmployee.EmployeeId, dto.EmployeeId);
        }

        [Fact]
        public async Task CreateEmployee_Duplicate_Returns400()
        {
            // Arrange
            var request = new CreateEmployeeDto(
                "John Doe",
                "+1234567890",
                "john@example.com",
                new DateOnly(1990, 1, 1),
                "photo.jpg",
                null);

            _mockService.Setup(x => x.AddEmployeeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ThrowsAsync(new EmployeeAlreadyExistsException("Duplicate"));

            // Act
            var result = await _controller.CreateEmployee(request);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("EmployeeAlreadyExistsException", errorDto.ErrorType);
        }

        [Fact]
        public async Task CreateEmployee_InvalidData_Returns400()
        {
            // Arrange
            var invalidRequest = new CreateEmployeeDto(
                "", // Invalid name
                "+1234567890",
                "john@example.com",
                new DateOnly(1990, 1, 1),
                "photo.jpg",
                null);

            _mockService.Setup(x => x.AddEmployeeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Invalid name"));

            // Act
            var result = await _controller.CreateEmployee(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }
        #endregion

        #region UpdateEmployee Tests
        [Fact]
        public async Task UpdateEmployee_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new UpdateEmployeeDto(
                Guid.NewGuid(),
                "Updated Name",
                "+1234567890",
                "updated@example.com",
                new DateOnly(1990, 1, 1),
                "new_photo.jpg",
                JsonSerializer.Serialize(new { Duty = "Updated" }));

            var expectedEmployee = new BaseEmployee(
                request.Id,
                request.FullName!,
                request.PhoneNumber!,
                request.Email!,
                request.Birthday!.Value,
                request.PhotoPath,
                request.Duties);

            _mockService.Setup(x => x.UpdateEmployeeAsync(
                request.Id,
                request.FullName,
                request.PhoneNumber,
                request.Email,
                request.Birthday,
                request.PhotoPath,
                request.Duties))
                .ReturnsAsync(expectedEmployee);

            // Act
            var result = await _controller.UpdateEmployee(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<EmployeeDto>(okResult.Value);
            Assert.Equal(request.Id, dto.EmployeeId);
            Assert.Equal("Updated Name", dto.FullName);
        }

        [Fact]
        public async Task UpdateEmployee_NotFound_Returns404()
        {
            // Arrange
            var request = new UpdateEmployeeDto(
                Guid.NewGuid(),
                "Updated Name",
                null, null, null, null, null);

            _mockService.Setup(x => x.UpdateEmployeeAsync(
                request.Id,
                request.FullName,
                request.PhoneNumber,
                request.Email,
                request.Birthday,
                request.PhotoPath,
                request.Duties))
                .ThrowsAsync(new EmployeeNotFoundException("Not found"));

            // Act
            var result = await _controller.UpdateEmployee(request);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("EmployeeNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region DeleteEmployee Tests
        [Fact]
        public async Task DeleteEmployee_ValidId_Returns204()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockService.Setup(x => x.DeleteEmployeeAsync(employeeId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteEmployee(employeeId);

            // Assert
            Assert.IsType<StatusCodeResult>(result);
        }

        [Fact]
        public async Task DeleteEmployee_NotFound_Returns404()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockService.Setup(x => x.DeleteEmployeeAsync(employeeId))
                .ThrowsAsync(new EmployeeNotFoundException("Not found"));

            // Act
            var result = await _controller.DeleteEmployee(employeeId);

            // Assert
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(notFoundResult.Value);
            Assert.Equal("EmployeeNotFoundException", errorDto.ErrorType);
        }
        #endregion

        #region Edge Cases
        [Fact]
        public async Task CreateEmployee_InvalidDutiesJson_Returns400()
        {
            // Arrange
            var invalidRequest = new CreateEmployeeDto(
                "John Doe",
                "+1234567890",
                "john@example.com",
                new DateOnly(1990, 1, 1),
                "photo.jpg",
                "{ invalid json }");

            _mockService.Setup(x => x.AddEmployeeAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Invalid duties JSON"));

            // Act
            var result = await _controller.CreateEmployee(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }

        [Fact]
        public async Task UpdateEmployee_FutureBirthDate_Returns400()
        {
            // Arrange
            var invalidRequest = new UpdateEmployeeDto(
                Guid.NewGuid(),
                null,
                null,
                null,
                DateOnly.FromDateTime(DateTime.Today.AddDays(1)), // Future date
                null,
                null);

            _mockService.Setup(x => x.UpdateEmployeeAsync(
                invalidRequest.Id,
                invalidRequest.FullName,
                invalidRequest.PhoneNumber,
                invalidRequest.Email,
                invalidRequest.Birthday,
                invalidRequest.PhotoPath,
                invalidRequest.Duties))
                .ThrowsAsync(new ArgumentException("Invalid birth date"));

            // Act
            var result = await _controller.UpdateEmployee(invalidRequest);

            // Assert
            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
            var errorDto = Assert.IsType<ErrorDto>(badRequestResult.Value);
            Assert.Equal("ArgumentException", errorDto.ErrorType);
        }
        #endregion
    }
}