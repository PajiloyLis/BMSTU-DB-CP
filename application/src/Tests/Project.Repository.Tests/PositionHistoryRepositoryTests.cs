using Database.Context;
using Database.Models;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models.PositionHistory;
using Project.Database.Models;
using Project.Database.Repositories;
using Testcontainers.PostgreSql;
using Xunit;

namespace Project.Repository.Tests;

public class PositionHistoryRepositoryTests : IAsyncLifetime
{
    private readonly Mock<ILogger<PositionHistoryRepository>> _mockLogger;
    private readonly PostgreSqlContainer _postgresContainer;

    private Guid _companyId1;
    private ProjectDbContext _context;
    private Guid _employeeId1;
    private Guid _employeeId2;
    private Guid _employeeId3;
    private Guid _employeeId4;
    private Guid _positionId1;
    private Guid _positionId2;
    private Guid _positionId3;
    private Guid _positionId4;
    private PositionHistoryRepository _repository;

    public PositionHistoryRepositoryTests()
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

        _mockLogger = new Mock<ILogger<PositionHistoryRepository>>();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        _context = new ProjectDbContext(options);
        // await Task.Delay(2);

        await _context.Database.MigrateAsync();

        _companyId1 = Guid.NewGuid();
        _employeeId1 = Guid.NewGuid();
        _employeeId2 = Guid.NewGuid();
        _employeeId3 = Guid.NewGuid();
        _employeeId4 = Guid.NewGuid();
        _positionId1 = Guid.NewGuid();
        _positionId2 = Guid.NewGuid();
        _positionId3 = Guid.NewGuid();
        _positionId4 = Guid.NewGuid();

        var company = new CompanyDb(_companyId1,
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");
        await _context.CompanyDb.AddAsync(company);

        var employees = new List<EmployeeDb>
        {
            new(_employeeId1, "John Doe",
                "+1234567890",
                "john.doe@example.com",
                new DateOnly(1990, 1, 1),
                "johnphoto.jpg",
                "{\"Developer\": true}"),
            new(_employeeId2, "Jason Doe",
                "+1234567899",
                "jason.doe@example.com",
                new DateOnly(1991, 1, 1),
                "jasonphoto.jpg",
                "{\"Developer\": true}"
            ),
            new(_employeeId3, "Jack Doe",
                "+1234467890",
                "jack.doe@example.com",
                new DateOnly(1989, 1, 1),
                "jackphoto.jpg",
                "{\"HR Manager\": true}"),
            new(_employeeId4, "George Doe",
                "+91234567890",
                "george.doe@example.com",
                new DateOnly(1995, 2, 1),
                "georgephoto.jpg",
                null)
        };
        await _context.EmployeeDb.AddRangeAsync(employees);


        var positions = new List<PositionDb>
        {
            new(_positionId1, null, "Manager", company.Id),
            new(_positionId2, _positionId1, "Senior Developer", company.Id),
            new(_positionId3, _positionId2, "Middle Developer", company.Id),
            new(_positionId4, _positionId3, "Junior Developer", company.Id)
        };
        await _context.PositionDb.AddRangeAsync(positions);

        await _context.SaveChangesAsync();


        _repository = new PositionHistoryRepository(_context, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddPositionHistoryAsync_ShouldAddAndReturnPositionHistory()
    {
        // Arrange

        var createDto = new CreatePositionHistory(_positionId1, _employeeId1, DateOnly.FromDateTime(DateTime.Today).AddDays(-1));

        // Act
        var result = await _repository.AddPositionHistoryAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_employeeId1, result.EmployeeId);
        Assert.Equal(_positionId1, result.PositionId);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today).AddDays(-1), result.StartDate);

        var dbEntry = await _context.PositionHistoryDb.FirstOrDefaultAsync();
        Assert.NotNull(dbEntry);
        Assert.Equal(_positionId1, dbEntry.PositionId);
        Assert.Equal(_employeeId1, dbEntry.EmployeeId);
    }

    [Fact]
    public async Task GetPositionHistoryByIdAsync_ShouldReturnPositionHistory_WhenExists()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);

        var entity = new PositionHistoryDb(_positionId1, _employeeId1, startDate);
        await _context.PositionHistoryDb.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPositionHistoryByIdAsync(_positionId1, _employeeId1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_positionId1, result.PositionId);
        Assert.Equal(_employeeId1, result.EmployeeId);
        Assert.Equal(startDate, result.StartDate);
    }

    [Fact]
    public async Task GetPositionHistoryByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<PositionHistoryNotFoundException>(() =>
            _repository.GetPositionHistoryByIdAsync(_positionId1, _employeeId1));
    }

    [Fact]
    public async Task UpdatePositionHistoryAsync_ShouldUpdateFields_WhenExists()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-5);
        var newEndDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);

        var entity = new PositionHistoryDb(_positionId1, _employeeId1, startDate);
        await _context.PositionHistoryDb.AddAsync(entity);
        await _context.SaveChangesAsync();

        var updateDto = new UpdatePositionHistory(_positionId1, _employeeId1, startDate, newEndDate);

        // Act
        var result = await _repository.UpdatePositionHistoryAsync(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newEndDate, result.EndDate);
        
        var dbEntry = await _context.PositionHistoryDb.FirstOrDefaultAsync();
        Assert.NotNull(dbEntry);
        Assert.Equal(newEndDate, dbEntry.EndDate);
    }

    [Fact]
    public async Task DeletePositionHistoryAsync_ShouldRemoveEntry_WhenExists()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);

        var entity = new PositionHistoryDb(_positionId1, _employeeId1, startDate);
        await _context.PositionHistoryDb.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeletePositionHistoryAsync(_positionId1, _employeeId1);

        // Assert
        var dbEntry = await _context.PositionHistoryDb.FirstOrDefaultAsync();
        Assert.Null(dbEntry);
    }

    [Fact]
    public async Task GetCurrentEmployeePositionByEmployeeIdAsync_ShouldReturnPosition_WhenExists()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        var entity = new PositionHistoryDb(_positionId1, _employeeId1, startDate);
        await _context.PositionHistoryDb.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCurrentEmployeePositionByEmployeeIdAsync(_employeeId1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_employeeId1, result.EmployeeId);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(_positionId1, result.PositionId);
        Assert.Null(result.EndDate);
    }

    [Fact]
    public async Task GetPositionHistoryByEmployeeIdAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var history = new List<PositionHistoryDb>
        {
            new(_positionId4, _employeeId1, new DateOnly(2018, 1, 1), new DateOnly(2020, 1, 1)),
            new(_positionId3, _employeeId1, new DateOnly(2020, 1, 1), new DateOnly(2022, 1, 1)),
            new(_positionId2, _employeeId1, new DateOnly(2022, 1, 1), new DateOnly(2024, 1, 1)),
            new(_positionId1, _employeeId1, new DateOnly(2024, 1, 1))
        };

        await _context.AddRangeAsync(history);
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPositionHistoryByEmployeeIdAsync(
            _employeeId1,
            1,
            2,
            DateOnly.MinValue, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(4, result.Page.TotalItems);
        Assert.Equal(2, result.Page.TotalPages);
    }

    [Fact]
    public async Task GetCurrentSubordinatesPositionHistoryAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var hierarchy = new List<PositionHistoryDb>
            { new (_positionId1, _employeeId1, new DateOnly(2015, 1, 1)),
        new (_positionId2, _employeeId2, new DateOnly(2015, 1, 1), new DateOnly(2020, 1, 1)),
        new (_positionId3, _employeeId3, new DateOnly(2015, 1, 1), new DateOnly(2020, 1, 1)),
        new (_positionId4, _employeeId4, new DateOnly(2015, 1, 1), new DateOnly(2020, 1, 1)),
        new (_positionId2, _employeeId4, new DateOnly(2020, 1, 1), new DateOnly(2025, 1, 1)),
        new (_positionId3, _employeeId2, new DateOnly(2020, 1, 1), new DateOnly(2025, 1, 1)),
        new (_positionId4, _employeeId3, new DateOnly(2020, 1, 1), new DateOnly(2025, 1, 1)),
        new (_positionId2, _employeeId3, new DateOnly(2025, 1, 1)),
        new (_positionId3, _employeeId4, new DateOnly(2025, 1, 1)),
        new (_positionId4, _employeeId2, new DateOnly(2025, 1, 1))
        };
        
        await _context.PositionHistoryDb.AddRangeAsync(hierarchy);
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCurrentSubordinatesAsync(
            _employeeId1,
            1,
            10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Items.Count);
        Assert.Equal(4, result.Page.TotalItems);
        Assert.Equal(1, result.Page.TotalPages);
    }
}