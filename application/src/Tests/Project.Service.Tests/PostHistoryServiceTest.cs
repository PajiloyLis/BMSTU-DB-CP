using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.PostHistory;
using Project.Core.Repositories;
using Project.Services.PostHistoryService;
using Xunit;

namespace Project.Service.Tests;

public class PostHistoryServiceTests
{
    private readonly Mock<ILogger<PostHistoryService>> _mockLogger;
    private readonly Mock<IPostHistoryRepository> _mockRepository;
    private readonly PostHistoryService _postHistoryService;

    public PostHistoryServiceTests()
    {
        _mockRepository = new Mock<IPostHistoryRepository>();
        _mockLogger = new Mock<ILogger<PostHistoryService>>();
        _postHistoryService = new PostHistoryService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AddPostHistory_Successful()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var startDate = new DateOnly(2024, 1, 1);
        var endDate = new DateOnly(2024, 12, 31);

        var createPostHistory = new CreatePostHistory(
            postId,
            employeeId,
            startDate,
            endDate
        );

        var expectedPostHistory = new BasePostHistory(
            postId,
            employeeId,
            startDate,
            endDate
        );

        _mockRepository.Setup(x => x.AddPostHistoryAsync(It.IsAny<CreatePostHistory>()))
            .ReturnsAsync(expectedPostHistory);

        //Act
        var result = await _postHistoryService.AddPostHistoryAsync(postId,
            employeeId,
            startDate,
            endDate);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPostHistory.PostId, result.PostId);
        Assert.Equal(expectedPostHistory.EmployeeId, result.EmployeeId);
        Assert.Equal(expectedPostHistory.StartDate, result.StartDate);
        Assert.Equal(expectedPostHistory.EndDate, result.EndDate);
        _mockRepository.Verify(x => x.AddPostHistoryAsync(It.IsAny<CreatePostHistory>()), Times.Once);
    }

    [Fact]
    public async Task GetPostHistoryById_Successful()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var expectedPostHistory = new BasePostHistory(
            postId,
            employeeId,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31)
        );

        _mockRepository.Setup(x => x.GetPostHistoryByIdAsync(postId, employeeId))
            .ReturnsAsync(expectedPostHistory);

        //Act
        var result = await _postHistoryService.GetPostHistoryAsync(postId, employeeId);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPostHistory.PostId, result.PostId);
        Assert.Equal(expectedPostHistory.EmployeeId, result.EmployeeId);
        _mockRepository.Verify(x => x.GetPostHistoryByIdAsync(postId, employeeId), Times.Once);
    }

    [Fact]
    public async Task GetPostHistoryById_NotFound()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetPostHistoryByIdAsync(postId, employeeId))
            .ThrowsAsync(new PostHistoryNotFoundException(postId, employeeId));

        //Act & Assert
        await Assert.ThrowsAsync<PostHistoryNotFoundException>(() =>
            _postHistoryService.GetPostHistoryAsync(postId, employeeId));
        _mockRepository.Verify(x => x.GetPostHistoryByIdAsync(postId, employeeId), Times.Once);
    }

    [Fact]
    public async Task UpdatePostHistory_Successful()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var updatePostHistory = new UpdatePostHistory(
            postId,
            employeeId,
            new DateOnly(2024, 2, 1),
            new DateOnly(2024, 12, 31)
        );

        var expectedPostHistory = new BasePostHistory(
            postId,
            employeeId,
            updatePostHistory.StartDate!.Value,
            updatePostHistory.EndDate
        );

        _mockRepository.Setup(x => x.UpdatePostHistoryAsync(It.IsAny<UpdatePostHistory>()))
            .ReturnsAsync(expectedPostHistory);

        //Act
        var result = await _postHistoryService.UpdatePostHistoryAsync(postId,
            employeeId,
            new DateOnly(2024, 2, 1),
            new DateOnly(2024, 12, 31));

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPostHistory.StartDate, result.StartDate);
        Assert.Equal(expectedPostHistory.EndDate, result.EndDate);
        _mockRepository.Verify(x => x.UpdatePostHistoryAsync(It.IsAny<UpdatePostHistory>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePostHistory_NotFound()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var updatePostHistory = new UpdatePostHistory(
            postId,
            employeeId,
            new DateOnly(2024, 2, 1),
            new DateOnly(2024, 12, 31)
        );

        _mockRepository.Setup(x => x.UpdatePostHistoryAsync(It.IsAny<UpdatePostHistory>()))
            .ThrowsAsync(new PostHistoryNotFoundException(postId, employeeId));

        //Act & Assert
        await Assert.ThrowsAsync<PostHistoryNotFoundException>(() =>
            _postHistoryService.UpdatePostHistoryAsync(postId,
                employeeId,
                new DateOnly(2024, 2, 1),
                new DateOnly(2024, 12, 31)));
        _mockRepository.Verify(x => x.UpdatePostHistoryAsync(It.IsAny<UpdatePostHistory>()), Times.Once);
    }

    [Fact]
    public async Task DeletePostHistory_Successful()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeletePostHistoryAsync(postId, employeeId))
            .Returns(Task.CompletedTask);

        //Act
        await _postHistoryService.DeletePostHistoryAsync(postId, employeeId);

        //Assert
        _mockRepository.Verify(x => x.DeletePostHistoryAsync(postId, employeeId), Times.Once);
    }

    [Fact]
    public async Task DeletePostHistory_NotFound()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeletePostHistoryAsync(postId, employeeId))
            .ThrowsAsync(new PostHistoryNotFoundException(postId, employeeId));

        //Act & Assert
        await Assert.ThrowsAsync<PostHistoryNotFoundException>(() =>
            _postHistoryService.DeletePostHistoryAsync(postId, employeeId));
        _mockRepository.Verify(x => x.DeletePostHistoryAsync(postId, employeeId), Times.Once);
    }

    [Fact]
    public async Task GetPostHistoryByEmployeeId_Successful()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 10;
        var startDate = new DateOnly(2024, 1, 1);
        var endDate = new DateOnly(2024, 12, 31);

        var expectedPostHistories = new List<BasePostHistory>
        {
            new(
                Guid.NewGuid(),
                employeeId,
                startDate,
                endDate
            ),
            new(
                Guid.NewGuid(),
                employeeId,
                startDate,
                endDate
            )
        };

        var expectedPage = new PostHistoryPage(
            expectedPostHistories,
            new Page(pageNumber, 2, pageSize)
        );

        _mockRepository.Setup(x =>
                x.GetPostHistoryByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate))
            .ReturnsAsync(expectedPage);

        //Act
        var result =
            await _postHistoryService.GetPostHistoryByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate,
                endDate);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Items.Count, result.Items.Count);
        _mockRepository.Verify(
            x => x.GetPostHistoryByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetSubordinatesPostHistory_Successful()
    {
        //Arrange
        var managerId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 10;
        var startDate = new DateOnly(2024, 1, 1);
        var endDate = new DateOnly(2024, 12, 31);

        var expectedPostHistories = new List<BasePostHistory>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                startDate,
                endDate
            ),
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                startDate,
                endDate
            )
        };

        var expectedPage = new PostHistoryPage(
            expectedPostHistories,
            new Page(pageNumber, 2, pageSize)
        );

        _mockRepository.Setup(x =>
                x.GetSubordinatesPostHistoryAsync(managerId, pageNumber, pageSize, startDate, endDate))
            .ReturnsAsync(expectedPage);

        //Act
        var result =
            await _postHistoryService.GetSubordinatesPostHistoryAsync(managerId, pageNumber, pageSize, startDate,
                endDate);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Items.Count, result.Items.Count);
        _mockRepository.Verify(
            x => x.GetSubordinatesPostHistoryAsync(managerId, pageNumber, pageSize, startDate, endDate), Times.Once);
    }
}