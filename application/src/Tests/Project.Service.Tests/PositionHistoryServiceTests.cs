using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models.PositionHistory;
using Project.Core.Repositories;
using Project.Services.PositionHistoryService;
using StackExchange.Redis;
using Xunit;

namespace Project.Service.Tests;

public class PositionHistoryServiceTests
{
    private readonly Mock<ILogger<PositionHistoryService>> _loggerMock;
    private readonly Mock<IPositionHistoryRepository> _repositoryMock;
    private readonly PositionHistoryService _service;
    private readonly Mock<IConnectionMultiplexer> _mockCache;

    public PositionHistoryServiceTests()
    {
        _repositoryMock = new Mock<IPositionHistoryRepository>();
        _loggerMock = new Mock<ILogger<PositionHistoryService>>();
        _mockCache = new Mock<IConnectionMultiplexer>();
        _service = new PositionHistoryService(_repositoryMock.Object, _loggerMock.Object, _mockCache.Object);
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PositionHistoryService(null!, _loggerMock.Object, _mockCache.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PositionHistoryService(_repositoryMock.Object, null!, _mockCache.Object));
    }

    [Fact]
    public async Task AddPositionHistoryAsync_ValidData_ReturnsPositionHistory()
    {
        // Arrange
        var positionId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var expectedPositionHistory = new BasePositionHistory(positionId, employeeId, startDate, endDate);

        _repositoryMock.Setup(x => x.AddPositionHistoryAsync(It.IsAny<CreatePositionHistory>()))
            .ReturnsAsync(expectedPositionHistory);

        // Act
        var result = await _service.AddPositionHistoryAsync(positionId, employeeId, startDate, endDate);

        // Assert
        Assert.Equal(expectedPositionHistory, result);
        _repositoryMock.Verify(x => x.AddPositionHistoryAsync(It.Is<CreatePositionHistory>(p =>
            p.PositionId == positionId &&
            p.EmployeeId == employeeId &&
            p.StartDate == startDate &&
            p.EndDate == endDate)), Times.Once);
    }


    [Fact]
    public async Task GetPositionHistoryAsync_NonExistingRecord_ThrowsPositionHistoryNotFoundException()
    {
        // Arrange
        var positionId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetPositionHistoryByIdAsync(positionId, employeeId))
            .ThrowsAsync(new PositionHistoryNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<PositionHistoryNotFoundException>(() =>
            _service.GetPositionHistoryAsync(positionId, employeeId));
    }

    [Fact]
    public async Task UpdatePositionHistoryAsync_ValidData_ReturnsUpdatedPositionHistory()
    {
        // Arrange
        var positionId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-90));
        var endDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        var expectedPositionHistory = new BasePositionHistory(positionId, employeeId, startDate, endDate);

        _repositoryMock.Setup(x => x.UpdatePositionHistoryAsync(It.IsAny<UpdatePositionHistory>()))
            .ReturnsAsync(expectedPositionHistory);

        // Act
        var result = await _service.UpdatePositionHistoryAsync(positionId, employeeId, startDate, endDate);

        // Assert
        Assert.Equal(expectedPositionHistory, result);
        _repositoryMock.Verify(x => x.UpdatePositionHistoryAsync(It.Is<UpdatePositionHistory>(p =>
            p.PositionId == positionId &&
            p.EmployeeId == employeeId &&
            p.StartDate == startDate &&
            p.EndDate == endDate)), Times.Once);
    }

    [Fact]
    public async Task UpdatePositionHistoryAsync_NonExistingRecord_ThrowsPositionHistoryNotFoundException()
    {
        // Arrange
        var positionId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.UpdatePositionHistoryAsync(It.IsAny<UpdatePositionHistory>()))
            .ThrowsAsync(new PositionHistoryNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<PositionHistoryNotFoundException>(() =>
            _service.UpdatePositionHistoryAsync(positionId, employeeId));
    }

    [Fact]
    public async Task DeletePositionHistoryAsync_ExistingRecord_DeletesSuccessfully()
    {
        // Arrange
        var positionId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.DeletePositionHistoryAsync(positionId, employeeId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeletePositionHistoryAsync(positionId, employeeId);

        // Assert
        _repositoryMock.Verify(x => x.DeletePositionHistoryAsync(positionId, employeeId), Times.Once);
    }

    [Fact]
    public async Task DeletePositionHistoryAsync_NonExistingRecord_ThrowsPositionHistoryNotFoundException()
    {
        // Arrange
        var positionId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.DeletePositionHistoryAsync(positionId, employeeId))
            .ThrowsAsync(new PositionHistoryNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<PositionHistoryNotFoundException>(() =>
            _service.DeletePositionHistoryAsync(positionId, employeeId));
    }
}