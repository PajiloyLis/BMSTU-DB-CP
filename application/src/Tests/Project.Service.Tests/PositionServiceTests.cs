using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Position;
using Project.Core.Models.PositionHistory;
using Project.Core.Repositories;
using Project.Services.PositionService;
using Xunit;

namespace Project.Service.Tests;

public class PositionServiceTests
{
    private readonly Mock<ILogger<PositionService>> _mockLogger;
    private readonly Mock<IPositionRepository> _mockRepository;
    private readonly PositionService _positionService;

    public PositionServiceTests()
    {
        _mockRepository = new Mock<IPositionRepository>();
        _mockLogger = new Mock<ILogger<PositionService>>();
        _positionService = new PositionService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AddPosition_Successful()
    {
        //Arrange
        var parentId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var title = "Software Engineer";

        var expectedPosition = new BasePosition(
            Guid.NewGuid(),
            parentId,
            title,
            companyId
        );

        _mockRepository.Setup(x => x.AddPositionAsync(It.IsAny<CreatePosition>()))
            .ReturnsAsync(expectedPosition);

        //Act
        var result = await _positionService.AddPositionAsync(parentId, title, companyId);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPosition.Id, result.Id);
        Assert.Equal(expectedPosition.ParentId, result.ParentId);
        Assert.Equal(expectedPosition.Title, result.Title);
        Assert.Equal(expectedPosition.CompanyId, result.CompanyId);
        _mockRepository.Verify(x => x.AddPositionAsync(It.IsAny<CreatePosition>()), Times.Once);
    }

    [Fact]
    public async Task GetPositionById_Successful()
    {
        //Arrange
        var positionId = Guid.NewGuid();
        var expectedPosition = new BasePosition(
            positionId,
            Guid.NewGuid(),
            "Software Engineer",
            Guid.NewGuid()
        );

        _mockRepository.Setup(x => x.GetPositionByIdAsync(positionId))
            .ReturnsAsync(expectedPosition);

        //Act
        var result = await _positionService.GetPositionByIdAsync(positionId);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPosition.Id, result.Id);
        Assert.Equal(expectedPosition.Title, result.Title);
        _mockRepository.Verify(x => x.GetPositionByIdAsync(positionId), Times.Once);
    }

    [Fact]
    public async Task GetPositionById_NotFound()
    {
        //Arrange
        var positionId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetPositionByIdAsync(positionId))
            .ThrowsAsync(new PositionNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<PositionNotFoundException>(() =>
            _positionService.GetPositionByIdAsync(positionId));
        _mockRepository.Verify(x => x.GetPositionByIdAsync(positionId), Times.Once);
    }

    [Fact]
    public async Task UpdatePosition_Successful()
    {
        //Arrange
        var positionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var title = "Senior Software Engineer";

        var expectedPosition = new BasePosition(
            positionId,
            parentId,
            title,
            companyId
        );

        _mockRepository.Setup(x => x.UpdatePositionAsync(It.IsAny<UpdatePosition>()))
            .ReturnsAsync(expectedPosition);

        //Act
        var result = await _positionService.UpdatePositionAsync(positionId, companyId, parentId, title);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPosition.Title, result.Title);
        Assert.Equal(expectedPosition.ParentId, result.ParentId);
        _mockRepository.Verify(x => x.UpdatePositionAsync(It.IsAny<UpdatePosition>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePosition_NotFound()
    {
        //Arrange
        var positionId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        _mockRepository.Setup(x => x.UpdatePositionAsync(It.IsAny<UpdatePosition>()))
            .ThrowsAsync(new PositionNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<PositionNotFoundException>(() =>
            _positionService.UpdatePositionAsync(positionId, companyId, null, "New Title"));
        _mockRepository.Verify(x => x.UpdatePositionAsync(It.IsAny<UpdatePosition>()), Times.Once);
    }

    [Fact]
    public async Task GetSubordinates_Successful()
    {
        //Arrange
        var parentId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 10;
        var expectedPositions = new List<PositionHierarchy>
        {
            new(
                Guid.NewGuid(),
                parentId,
                "Junior Software Engineer",
                1
            ),
            new(
                parentId,
                null,
                "Middle Software Engineer",
                0
            )
        };

        var expectedPage = new PositionHierarchyPage(
            expectedPositions,
            new Page(pageNumber, 2, pageSize)
        );

        _mockRepository.Setup(x => x.GetSubordinatesAsync(parentId, pageNumber, pageSize))
            .ReturnsAsync(expectedPage);

        //Act
        var result = await _positionService.GetSubordinatesAsync(parentId, pageNumber, pageSize);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Items.Count, result.Items.Count);
        _mockRepository.Verify(x => x.GetSubordinatesAsync(parentId, pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task DeletePosition_Successful()
    {
        //Arrange
        var positionId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeletePositionAsync(positionId))
            .Returns(Task.CompletedTask);

        //Act
        await _positionService.DeletePositionAsync(positionId);

        //Assert
        _mockRepository.Verify(x => x.DeletePositionAsync(positionId), Times.Once);
    }

    [Fact]
    public async Task DeletePosition_NotFound()
    {
        //Arrange
        var positionId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeletePositionAsync(positionId))
            .ThrowsAsync(new PositionNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<PositionNotFoundException>(() =>
            _positionService.DeletePositionAsync(positionId));
        _mockRepository.Verify(x => x.DeletePositionAsync(positionId), Times.Once);
    }

    [Fact]
    public async Task AddPosition_WithEmptyTitle_ThrowsException()
    {
        //Arrange
        var parentId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _positionService.AddPositionAsync(parentId, "", companyId));

        _mockRepository.Verify(x => x.AddPositionAsync(It.IsAny<CreatePosition>()), Times.Never);
    }
}