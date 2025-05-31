using Database.Context;
using Database.Models;
using Database.Repositories;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models.Score;
using Project.Database.Models;
using Testcontainers.PostgreSql;
using Xunit;

namespace Project.Repository.Tests;

public class ScoreRepositoryTests : IAsyncLifetime
{
    private readonly Mock<ILogger<ScoreRepository>> _mockLogger;
    private readonly PostgreSqlContainer _postgresContainer;
    private ProjectDbContext _context;
    private ScoreRepository _repository;

    public ScoreRepositoryTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("ppo_test")
            .WithUsername("test")
            .WithPassword("test")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5432))
            .Build();

        _mockLogger = new Mock<ILogger<ScoreRepository>>();

    }

    private Guid _employeeId;
    private Guid _employeeId2;
    private Guid _authorId;
    private Guid _directorId;
    private Guid _positionId;
    private Guid _lowestPositionId;
    private Guid _managerPositionId;
    
    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;
        
        _context = new ProjectDbContext(options);
        // await Task.Delay(2);

        await _context.Database.MigrateAsync();

        var company = new CompanyDb(Guid.NewGuid(),
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");
        await _context.CompanyDb.AddAsync(company);

        _employeeId = Guid.NewGuid();
        _employeeId2 = Guid.NewGuid();
        _authorId = Guid.NewGuid();
        _directorId = Guid.NewGuid();
        
        var employee = new EmployeeDb(_employeeId, "John Doe",
            "+1234567890",
            "john.doe@example.com",
            new DateOnly(1990, 1, 1),
            "johnphoto.jpg",
            "{\"Developer\": true}");
        var employee2 = new EmployeeDb(_employeeId2, "Jason Doe",
            "+1234567899",
            "jason.doe@example.com",
            new DateOnly(1991, 1, 1),
            "jasonphoto.jpg",
            "{\"Developer\": true}"
        );
        var author = new EmployeeDb(_authorId, "Jack Doe",
            "+1234467890",
            "jack.doe@example.com",
            new DateOnly(1989, 1, 1),
            "jackphoto.jpg",
            "{\"HR Manager\": true}");
        var director = new EmployeeDb(_directorId, "George Doe",
            "+91234567890",
            "george.doe@example.com",
            new DateOnly(1995, 2, 1),
            "georgephoto.jpg",
            null);
        await _context.EmployeeDb.AddRangeAsync(employee, employee2, director, author);

        _managerPositionId = Guid.NewGuid();
        _positionId = Guid.NewGuid();
        _lowestPositionId = Guid.NewGuid();
        
        var positions = new List<PositionDb>
        {
            new(_managerPositionId, null, "Manager", company.Id),
            new(_positionId, _managerPositionId, "Developer", company.Id),
            new(_lowestPositionId, _positionId, "Junior Developer", company.Id)
        };
        await _context.PositionDb.AddRangeAsync(positions);

        var positionsHistories = new List<PositionHistoryDb>
        {
            new(_lowestPositionId, _employeeId2, new DateOnly(2015, 9, 10), null),
            new(_positionId, _employeeId, new DateOnly(2016, 9, 10), null),
            new(_managerPositionId, _authorId, new DateOnly(2014, 9, 10), null),
        };
        
        await _context.PositionHistoryDb.AddRangeAsync(positionsHistories);

        await _context.SaveChangesAsync();

        _repository = new ScoreRepository(_context, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
    
    [Fact]
    public async Task AddScoreAsync_ShouldAddScore_WhenValid()
    {
        // Arrange
        var score = new CreateScore(_employeeId, _authorId, _positionId, 
            DateTimeOffset.UtcNow, 4, 5, 3);

        // Act
        var result = await _repository.AddScoreAsync(score);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.EfficiencyScore);
        Assert.Equal(_employeeId, result.EmployeeId);

        var dbRecord = await _context.ScoreDb.FindAsync(result.Id);
        Assert.NotNull(dbRecord);
    }

    [Fact]
    public async Task AddScoreAsync_ShouldThrow_WhenInvalidScores()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddScoreAsync( new CreateScore(_employeeId, _authorId, _positionId,
            DateTimeOffset.UtcNow, 6, 0, 10)));
    }

    [Fact]
    public async Task GetScoreByIdAsync_ShouldReturnScore_WhenExists()
    {
        // Arrange
        var score = new ScoreDb(Guid.NewGuid(), _employeeId, _authorId, _positionId, DateTimeOffset.UtcNow, 1, 2, 3);
        await _context.ScoreDb.AddAsync(score);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetScoreByIdAsync(score.Id);

        // Assert
        Assert.Equal(score.Id, result.Id);
        Assert.Equal(3, result.CompetencyScore);
    }

    [Fact]
    public async Task GetScoreByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ScoreNotFoundException>(
            () => _repository.GetScoreByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateScoreAsync_ShouldUpdateFields_WhenValid()
    {
        // Arrange
        var score = new ScoreDb(Guid.NewGuid(), _employeeId, _authorId, _positionId, DateTimeOffset.Now.AddHours(-3).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score);
        await _context.SaveChangesAsync();
        var update = new UpdateScore(score.Id, null, 5, null, 4); // Обновляем 2 из 3 оценок

        // Act
        var result = await _repository.UpdateScoreAsync(update);

        // Assert
        Assert.Equal(5, result.EfficiencyScore); // Обновлено
        Assert.Equal(4, result.CompetencyScore); // Обновлено
        Assert.Equal(2, result.EngagementScore); // Осталось прежним
    }

    [Fact]
    public async Task DeleteScoreAsync_ShouldRemoveScore_WhenExists()
    {
        // Arrange
        var score = new ScoreDb(Guid.NewGuid(), _employeeId, _authorId, _positionId, DateTimeOffset.Now.AddDays(-10).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteScoreAsync(score.Id);

        // Assert
        await Assert.ThrowsAsync<ScoreNotFoundException>(
            () => _repository.GetScoreByIdAsync(score.Id));
    }

    [Fact]
    public async Task GetScoresAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            var score = new ScoreDb(Guid.NewGuid(), _employeeId, _authorId, _positionId, DateTimeOffset.Now.AddDays(-i).ToUniversalTime(), 1, 2, 3);
            await _context.ScoreDb.AddAsync(score);
        }
        await _context.SaveChangesAsync();;
        
        // Act
        var page1 = await _repository.GetScoresAsync(1, 5, null, null);
        var page2 = await _repository.GetScoresAsync(2, 5, null, null);

        // Assert
        Assert.Equal(5, page1.Items.Count);
        Assert.Equal(5, page2.Items.Count);
        Assert.Equal(15, page1.Page.TotalItems);
    }

    [Fact]
    public async Task GetScoresByEmployeeIdAsync_ShouldFilterByEmployee()
    {
        // Arrange
        var score1 = new ScoreDb(Guid.NewGuid(), _employeeId, _authorId, _positionId, DateTimeOffset.Now.AddDays(-10).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score1);
        var score2 = new ScoreDb(Guid.NewGuid(), _employeeId2, _employeeId, _lowestPositionId, DateTimeOffset.Now.AddDays(-10).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetScoresByEmployeeIdAsync(_employeeId, 1, 10, null, null);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(score1.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task GetScoresByPositionIdAsync_ShouldFilterByPosition()
    {
        // Arrange
        var score1 = new ScoreDb(Guid.NewGuid(), _employeeId, _authorId, _positionId, DateTimeOffset.Now.AddDays(-10).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score1);
        var score2 = new ScoreDb(Guid.NewGuid(), _employeeId2, _employeeId, _lowestPositionId, DateTimeOffset.Now.AddDays(-10).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetScoresByPositionIdAsync(_positionId, 1, 10, null, null);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(score1.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task GetScoresByAuthorIdAsync_ShouldFilterByAuthor()
    {
        // Arrange
        var score1 = new ScoreDb(Guid.NewGuid(), _employeeId, _authorId, _positionId, DateTimeOffset.Now.AddDays(-10).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score1);
        var score2 = new ScoreDb(Guid.NewGuid(), _employeeId2, _employeeId, _lowestPositionId, DateTimeOffset.Now.AddDays(-10).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetScoresByAuthorIdAsync(_authorId, 1, 10, null, null);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(score1.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task GetScoresSubordinatesByEmployeeIdAsync_ShouldReturnSubordinatesScores()
    {
        // Arrange

        var random = new Random();
        
        for (int i = 0; i < 10; ++i)
        {
            var score0 = new ScoreDb(Guid.NewGuid(), _authorId, _directorId, _managerPositionId, DateTimeOffset.Now.AddDays(-i*30).ToUniversalTime(), random.Next(1, 6), random.Next(1, 6), random.Next(1, 6));
            var score1 = new ScoreDb(Guid.NewGuid(), _employeeId, _authorId, _positionId, DateTimeOffset.Now.AddDays(-i*30).ToUniversalTime(), random.Next(1, 6), random.Next(1, 6), random.Next(1, 6));
            var score2 = new ScoreDb(Guid.NewGuid(), _employeeId2, _employeeId, _lowestPositionId, DateTimeOffset.Now.AddDays(-i*30).ToUniversalTime(), random.Next(1, 6), random.Next(1, 6), random.Next(1, 6));
            await _context.ScoreDb.AddAsync(score1);
            await _context.ScoreDb.AddAsync(score2);
            await _context.ScoreDb.AddAsync(score0);
        }
        await _context.SaveChangesAsync();
    
        // Act
        var result = await _repository.GetScoresSubordinatesByEmployeeIdAsync(
            _employeeId, 1, 10, DateTimeOffset.Now.AddDays(-8*30).ToUniversalTime(),  DateTimeOffset.Now.AddDays(-2*30).ToUniversalTime());
    
        // Assert
        Assert.Equal(2, result.Page.TotalPages);
        Assert.Equal(10, result.Items.Count);
        Assert.DoesNotContain(_authorId, result.Items.Select(x => x.EmployeeId));
    }

    [Fact]
    public async Task GetScoresAsync_ShouldFilterByDateRange()
    {
        // Arrange
        var score1 = new ScoreDb(Guid.NewGuid(), _employeeId, _authorId, _positionId, DateTimeOffset.Now.AddDays(-1).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score1);
        var score2 = new ScoreDb(Guid.NewGuid(), _employeeId2, _employeeId, _lowestPositionId, DateTimeOffset.Now.AddDays(-40).ToUniversalTime(), 1, 2, 3);
        await _context.ScoreDb.AddAsync(score2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetScoresAsync(1, 10, 
            DateTimeOffset.UtcNow.AddDays(-3), 
            DateTimeOffset.UtcNow);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(score1.Id, result.Items[0].Id);
    }
}