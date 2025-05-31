using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Score;
using Project.Core.Repositories;
using Project.Services.ScoreService;
using Xunit;

namespace Project.Service.Tests;

public class ScoreServiceTests
{
    private readonly Mock<ILogger<ScoreService>> _mockLogger;
    private readonly Mock<IScoreRepository> _mockRepository;
    private readonly ScoreService _scoreService;

    public ScoreServiceTests()
    {
        _mockRepository = new Mock<IScoreRepository>();
        _mockLogger = new Mock<ILogger<ScoreService>>();
        _scoreService = new ScoreService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AddScore_Successful()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var createScore = new CreateScore(
            employeeId,
            authorId,
            positionId,
            DateTimeOffset.UtcNow,
            4,
            5,
            3
        );

        var expectedScore = new BaseScore(
            Guid.NewGuid(),
            employeeId,
            authorId,
            positionId,
            createScore.CreatedAt,
            createScore.EfficiencyScore,
            createScore.EngagementScore,
            createScore.CompetencyScore
        );

        _mockRepository.Setup(x => x.AddScoreAsync(It.IsAny<CreateScore>()))
            .ReturnsAsync(expectedScore);

        //Act
        var result = await _scoreService.AddScoreAsync(createScore.EmployeeId, createScore.AuthorId,
            createScore.PositionId, createScore.CreatedAt, createScore.EfficiencyScore, createScore.EngagementScore,
            createScore.CompetencyScore);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedScore.Id, result.Id);
        Assert.Equal(expectedScore.EmployeeId, result.EmployeeId);
        Assert.Equal(expectedScore.AuthorId, result.AuthorId);
        Assert.Equal(expectedScore.PositionId, result.PositionId);
        Assert.Equal(expectedScore.EfficiencyScore, result.EfficiencyScore);
        Assert.Equal(expectedScore.EngagementScore, result.EngagementScore);
        Assert.Equal(expectedScore.CompetencyScore, result.CompetencyScore);
        _mockRepository.Verify(x => x.AddScoreAsync(It.IsAny<CreateScore>()), Times.Once);
    }

    [Fact]
    public async Task GetScoreById_Successful()
    {
        //Arrange
        var scoreId = Guid.NewGuid();
        var expectedScore = new BaseScore(
            scoreId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            4,
            5,
            3
        );

        _mockRepository.Setup(x => x.GetScoreByIdAsync(scoreId))
            .ReturnsAsync(expectedScore);

        //Act
        var result = await _scoreService.GetScoreAsync(scoreId);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedScore.Id, result.Id);
        Assert.Equal(expectedScore.EmployeeId, result.EmployeeId);
        Assert.Equal(expectedScore.EfficiencyScore, result.EfficiencyScore);
        _mockRepository.Verify(x => x.GetScoreByIdAsync(scoreId), Times.Once);
    }

    [Fact]
    public async Task GetScoreById_NotFound()
    {
        //Arrange
        var scoreId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetScoreByIdAsync(scoreId))
            .ThrowsAsync(new ScoreNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<ScoreNotFoundException>(() =>
            _scoreService.GetScoreAsync(scoreId));
        _mockRepository.Verify(x => x.GetScoreByIdAsync(scoreId), Times.Once);
    }

    [Fact]
    public async Task UpdateScore_Successful()
    {
        //Arrange
        var scoreId = Guid.NewGuid();
        var updateScore = new UpdateScore(
            scoreId,
            DateTimeOffset.UtcNow,
            5,
            4,
            3
        );

        var expectedScore = new BaseScore(
            scoreId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            updateScore.CreatedAt!.Value,
            updateScore.EfficiencyScore!.Value,
            updateScore.EngagementScore!.Value,
            updateScore.CompetencyScore!.Value
        );

        _mockRepository.Setup(x => x.UpdateScoreAsync(It.IsAny<UpdateScore>()))
            .ReturnsAsync(expectedScore);

        //Act
        var result = await _scoreService.UpdateScoreAsync(updateScore.Id, updateScore.CreatedAt, updateScore.EfficiencyScore, updateScore.EngagementScore, updateScore.CompetencyScore);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedScore.EfficiencyScore, result.EfficiencyScore);
        Assert.Equal(expectedScore.EngagementScore, result.EngagementScore);
        Assert.Equal(expectedScore.CompetencyScore, result.CompetencyScore);
        _mockRepository.Verify(x => x.UpdateScoreAsync(It.IsAny<UpdateScore>()), Times.Once);
    }

    [Fact]
    public async Task UpdateScore_NotFound()
    {
        //Arrange
        var scoreId = Guid.NewGuid();
        var updateScore = new UpdateScore(scoreId, null, 5);
        _mockRepository.Setup(x => x.UpdateScoreAsync(It.IsAny<UpdateScore>()))
            .ThrowsAsync(new ScoreNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<ScoreNotFoundException>(() =>
            _scoreService.UpdateScoreAsync(updateScore.Id, updateScore.CreatedAt, updateScore.EfficiencyScore, updateScore.EngagementScore, updateScore.CompetencyScore));
        _mockRepository.Verify(x => x.UpdateScoreAsync(It.IsAny<UpdateScore>()), Times.Once);
    }

    [Fact]
    public async Task GetScores_Successful()
    {
        //Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var startDate = DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = DateTimeOffset.UtcNow;

        var expectedScores = new List<BaseScore>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                4,
                5,
                3
            ),
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                5,
                4,
                3
            )
        };

        var expectedPage = new ScorePage(
            expectedScores,
            new Page(pageNumber, 2, pageSize)
        );

        _mockRepository.Setup(x => x.GetScoresAsync(pageNumber, pageSize, startDate, endDate))
            .ReturnsAsync(expectedPage);

        //Act
        var result = await _scoreService.GetScoresAsync(pageNumber, pageSize, startDate, endDate);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Items.Count, result.Items.Count);
        _mockRepository.Verify(x => x.GetScoresAsync(pageNumber, pageSize, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetScoresByEmployee_Successful()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 10;
        var startDate = DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = DateTimeOffset.UtcNow;

        var expectedScores = new List<BaseScore>
        {
            new(
                Guid.NewGuid(),
                employeeId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                4,
                5,
                3
            )
        };

        var expectedPage = new ScorePage(
            expectedScores,
            new Page(pageNumber, 1, pageSize)
        );

        _mockRepository.Setup(x => x.GetScoresByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate))
            .ReturnsAsync(expectedPage);

        //Act
        var result = await _scoreService.GetScoresByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Items.Count, result.Items.Count);
        _mockRepository.Verify(x => x.GetScoresByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate),
            Times.Once);
    }

    [Fact]
    public async Task GetScoresByAuthor_Successful()
    {
        //Arrange
        var authorId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 10;
        var startDate = DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = DateTimeOffset.UtcNow;

        var expectedScores = new List<BaseScore>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                authorId,
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                4,
                5,
                3
            )
        };

        var expectedPage = new ScorePage(
            expectedScores,
            new Page(pageNumber, 1, pageSize)
        );

        _mockRepository.Setup(x => x.GetScoresByAuthorIdAsync(authorId, pageNumber, pageSize, startDate, endDate))
            .ReturnsAsync(expectedPage);

        //Act
        var result = await _scoreService.GetScoresByAuthorIdAsync(authorId, pageNumber, pageSize, startDate, endDate);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Items.Count, result.Items.Count);
        _mockRepository.Verify(x => x.GetScoresByAuthorIdAsync(authorId, pageNumber, pageSize, startDate, endDate),
            Times.Once);
    }

    [Fact]
    public async Task GetScoresByPosition_Successful()
    {
        //Arrange
        var positionId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 10;
        var startDate = DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = DateTimeOffset.UtcNow;

        var expectedScores = new List<BaseScore>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                positionId,
                DateTimeOffset.UtcNow,
                4,
                5,
                3
            )
        };

        var expectedPage = new ScorePage(
            expectedScores,
            new Page(pageNumber, 1, pageSize)
        );

        _mockRepository.Setup(x => x.GetScoresByPositionIdAsync(positionId, pageNumber, pageSize, startDate, endDate))
            .ReturnsAsync(expectedPage);

        //Act
        var result =
            await _scoreService.GetScoresByPositionIdAsync(positionId, pageNumber, pageSize, startDate, endDate);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Items.Count, result.Items.Count);
        _mockRepository.Verify(x => x.GetScoresByPositionIdAsync(positionId, pageNumber, pageSize, startDate, endDate),
            Times.Once);
    }

    [Fact]
    public async Task GetScoresSubordinatesByEmployee_Successful()
    {
        //Arrange
        var employeeId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 10;
        var startDate = DateTimeOffset.UtcNow.AddDays(-30);
        var endDate = DateTimeOffset.UtcNow;

        var expectedScores = new List<BaseScore>
        {
            new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow,
                4,
                5,
                3
            )
        };

        var expectedPage = new ScorePage(
            expectedScores,
            new Page(pageNumber, 1, pageSize)
        );

        _mockRepository.Setup(x =>
                x.GetScoresSubordinatesByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate))
            .ReturnsAsync(expectedPage);

        //Act
        var result =
            await _scoreService.GetScoresSubordinatesByEmployeeAsync(employeeId, pageNumber, pageSize, startDate,
                endDate);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Items.Count, result.Items.Count);
        _mockRepository.Verify(
            x => x.GetScoresSubordinatesByEmployeeIdAsync(employeeId, pageNumber, pageSize, startDate, endDate),
            Times.Once);
    }

    [Fact]
    public async Task DeleteScore_Successful()
    {
        //Arrange
        var scoreId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeleteScoreAsync(scoreId))
            .Returns(Task.CompletedTask);

        //Act
        await _scoreService.DeleteScoreAsync(scoreId);

        //Assert
        _mockRepository.Verify(x => x.DeleteScoreAsync(scoreId), Times.Once);
    }

    [Fact]
    public async Task DeleteScore_NotFound()
    {
        //Arrange
        var scoreId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeleteScoreAsync(scoreId))
            .ThrowsAsync(new ScoreNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<ScoreNotFoundException>(() =>
            _scoreService.DeleteScoreAsync(scoreId));
        _mockRepository.Verify(x => x.DeleteScoreAsync(scoreId), Times.Once);
    }
}