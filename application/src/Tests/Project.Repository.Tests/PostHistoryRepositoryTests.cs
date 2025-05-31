using Database.Context;
using Database.Models;
using Database.Repositories;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models.PostHistory;
using Project.Database.Models;
using Testcontainers.PostgreSql;
using Xunit;

namespace Project.Repository.Tests;

public class PostHistoryRepositoryTests : IAsyncLifetime
{
    private readonly Mock<ILogger<PostHistoryRepository>> _mockLogger;
    private readonly PostgreSqlContainer _postgresContainer;

    private Guid _companyId1;
    private ProjectDbContext _context;
    private Guid _employeeId1;
    private Guid _employeeId2;
    private Guid _employeeId3;
    private Guid _employeeId4;
    private Guid _postId1;
    private Guid _postId2;
    private Guid _postId3;
    private Guid _postId4;
    private Guid _positionId1;
    private Guid _positionId2;
    private Guid _positionId3;
    private Guid _positionId4;
    private PostHistoryRepository _repository;

    public PostHistoryRepositoryTests()
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

        _mockLogger = new Mock<ILogger<PostHistoryRepository>>();
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
        _postId1 = Guid.NewGuid();
        _postId2 = Guid.NewGuid();
        _postId3 = Guid.NewGuid();
        _postId4 = Guid.NewGuid();
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


        var posts = new List<PostDb>
        {
            new(_postId1, "Manager", 200000, company.Id),
            new(_postId2,  "Senior Developer", 195000, company.Id),
            new(_postId3, "Middle Developer", 150000, company.Id),
            new(_postId4, "Junior Developer", 100000, company.Id)
        };
        await _context.PostDb.AddRangeAsync(posts);

        var positions = new List<PositionDb>
        {
            new(_positionId1, null, "Manager", company.Id),
            new(_positionId2, _positionId1, "Senior Developer", company.Id),
            new(_positionId3, _positionId2, "Middle Developer", company.Id),
            new(_positionId4, _positionId3, "Junior Developer", company.Id)
        };
        await _context.PositionDb.AddRangeAsync(positions);
        
        await _context.SaveChangesAsync();


        _repository = new PostHistoryRepository(_context, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
    
    [Fact]
    public async Task AddPostHistoryAsync_ShouldAddNewPostHistory()
    {
        // Arrange
        var createDto = new CreatePostHistory(_postId1, _employeeId1, DateOnly.FromDateTime(DateTime.Today).AddDays(-1), null);

        // Act
        var result = await _repository.AddPostHistoryAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_postId1, result.PostId);
        Assert.Equal(_employeeId1, result.EmployeeId);
        Assert.Equal(createDto.StartDate, result.StartDate);
        Assert.Null(result.EndDate);
        
        var dbEntry = await _context.PostHistoryDb.FirstOrDefaultAsync();
        Assert.NotNull(dbEntry);
        Assert.Equal(_postId1, dbEntry.PostId);
        Assert.Equal(_employeeId1, dbEntry.EmployeeId);
        Assert.Equal(createDto.StartDate, dbEntry.StartDate);
        Assert.Null(dbEntry.EndDate);
    }

    [Fact]
    public async Task GetPostHistoryByIdAsync_ShouldReturnPostHistory_WhenExists()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        var entity = new PostHistoryDb(_postId1, _employeeId1, startDate);
        await _context.PostHistoryDb.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPostHistoryByIdAsync(_postId1, _employeeId1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_postId1, result.PostId);
        Assert.Equal(_employeeId1, result.EmployeeId);
        Assert.Equal(startDate, result.StartDate);
        Assert.Null(result.EndDate);
    }

    [Fact]
    public async Task GetPostHistoryByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<PostHistoryNotFoundException>(() => 
            _repository.GetPostHistoryByIdAsync(_postId1, _employeeId1));
    }

    [Fact]
    public async Task UpdatePostHistoryAsync_ShouldUpdateDates_WhenExists()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today).AddDays(-10);
        var newEndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        
        var entity = new PostHistoryDb(_postId1, _employeeId1, startDate);
        await _context.PostHistoryDb.AddAsync(entity);
        await _context.SaveChangesAsync();

        var updateDto = new UpdatePostHistory(_postId1, _employeeId1, null, newEndDate);

        // Act
        var result = await _repository.UpdatePostHistoryAsync(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_postId1, result.PostId);
        Assert.Equal(_employeeId1, result.EmployeeId);
        Assert.Equal(newEndDate, result.EndDate);
        Assert.Equal(startDate, result.StartDate);

        var dbEntry = await _context.PostHistoryDb.FirstOrDefaultAsync();
        Assert.NotNull(dbEntry);
        Assert.Equal(_postId1, dbEntry.PostId);
        Assert.Equal(_employeeId1, dbEntry.EmployeeId);
        Assert.Equal(newEndDate, dbEntry.EndDate);
        Assert.Equal(startDate, dbEntry.StartDate);
    }

    [Fact]
    public async Task DeletePostHistoryAsync_ShouldRemoveEntry_WhenExists()
    {
        // Arrange
        var entity = new PostHistoryDb(_postId1, _employeeId1, DateOnly.FromDateTime(DateTime.Today).AddDays(-1));
        await _context.PostHistoryDb.AddAsync(entity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeletePostHistoryAsync(_postId1, _employeeId1);

        // Assert
        var dbEntry = await _context.PostHistoryDb.FirstOrDefaultAsync();
        Assert.Null(dbEntry);
    }

    [Fact]
    public async Task GetPostHistoryByEmployeeIdAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-50));
        
        // Добавляем несколько записей истории
        var histories = new List<PostHistoryDb>
        {
            new(_postId1, _employeeId1, startDate, startDate.AddDays(10)),
            new(_postId2, _employeeId1, startDate.AddDays(11), startDate.AddDays(20)),
            new(_postId3, _employeeId1, startDate.AddDays(21))
        };
        
        await _context.PostHistoryDb.AddRangeAsync(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPostHistoryByEmployeeIdAsync(
            _employeeId1, 
            pageNumber: 1, 
            pageSize: 2,
            startDate: null,
            endDate: null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.Page.TotalItems);
        Assert.Equal(2, result.Page.TotalPages);
    }

    [Fact]
    public async Task GetPostHistoryByEmployeeIdAsync_ShouldFilterByDateRange()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-50));
        var filterStartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-35));
        
        var histories = new List<PostHistoryDb>
        {
            new(_postId1, _employeeId1, startDate, startDate.AddDays(10)), // До фильтра
            new(_postId2, _employeeId1, startDate.AddDays(11), startDate.AddDays(20)), // Входит в фильтр
            new(_postId3, _employeeId1, startDate.AddDays(21)) // Входит в фильтр (текущая)
        };
        
        await _context.PostHistoryDb.AddRangeAsync(histories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPostHistoryByEmployeeIdAsync(
            _employeeId1, 
            pageNumber: 1, 
            pageSize: 10,
            startDate: filterStartDate,
            endDate: null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page.TotalItems);
        Assert.Contains(_postId2, result.Items.Select(x => x.PostId));
        Assert.Contains(_postId3, result.Items.Select(x => x.PostId));
        Assert.DoesNotContain(_postId1, result.Items.Select(x => x.PostId));
    }

    [Fact]
    public async Task GetSubordinatesPostHistoryAsync_ShouldReturnManagerSubordinatesHistory()
    {
        // Arrange
        var hierarchy = new List<PostHistoryDb>
        { new (_postId1, _employeeId1, new DateOnly(2015, 1, 1)),
            new (_postId2, _employeeId2, new DateOnly(2015, 1, 1), new DateOnly(2020, 1, 1)),
            new (_postId3, _employeeId3, new DateOnly(2015, 1, 1), new DateOnly(2020, 1, 1)),
            new (_postId4, _employeeId4, new DateOnly(2015, 1, 1), new DateOnly(2020, 1, 1)),
            new (_postId2, _employeeId4, new DateOnly(2020, 1, 1), new DateOnly(2025, 1, 1)),
            new (_postId3, _employeeId2, new DateOnly(2020, 1, 1), new DateOnly(2025, 1, 1)),
            new (_postId4, _employeeId3, new DateOnly(2020, 1, 1), new DateOnly(2025, 1, 1)),
            new (_postId2, _employeeId3, new DateOnly(2025, 1, 1)),
            new (_postId3, _employeeId4, new DateOnly(2025, 1, 1)),
            new (_postId4, _employeeId2, new DateOnly(2025, 1, 1))
        };
        
        await _context.PostHistoryDb.AddRangeAsync(hierarchy);
        var hierarchy2 = new List<PositionHistoryDb>
        { new (_positionId1, _employeeId1, new DateOnly(2015, 1, 1)),
            new (_positionId2, _employeeId3, new DateOnly(2025, 1, 1)),
            new (_positionId3, _employeeId4, new DateOnly(2025, 1, 1)),
            new (_positionId4, _employeeId2, new DateOnly(2025, 1, 1))
        };
        
        await _context.PositionHistoryDb.AddRangeAsync(hierarchy2);
        
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetSubordinatesPostHistoryAsync(
            _employeeId1, 
            pageNumber: 1, 
            pageSize: 3,
            startDate: null,
            endDate: null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(10, result.Page.TotalItems);
        Assert.Equal(4, result.Page.TotalPages);
    }

    [Fact]
    public async Task GetSubordinatesPostHistoryAsync_ShouldFilterByDateRange()
    {
        // Arrange
        var filterStartDate = new DateOnly(2020, 2, 1);
        var hierarchy = new List<PostHistoryDb>
        { new (_postId1, _employeeId1, new DateOnly(2015, 1, 1)),
            new (_postId2, _employeeId2, new DateOnly(2015, 1, 1), new DateOnly(2020, 1, 1)),
            new (_postId3, _employeeId3, new DateOnly(2015, 1, 1), new DateOnly(2020, 1, 1)),
            new (_postId4, _employeeId4, new DateOnly(2015, 1, 1), new DateOnly(2020, 1, 1)),
            new (_postId2, _employeeId4, new DateOnly(2020, 1, 1), new DateOnly(2025, 1, 1)),
            new (_postId3, _employeeId2, new DateOnly(2020, 1, 1), new DateOnly(2025, 1, 1)),
            new (_postId4, _employeeId3, new DateOnly(2020, 1, 1), new DateOnly(2025, 1, 1)),
            new (_postId2, _employeeId3, new DateOnly(2025, 1, 1)),
            new (_postId3, _employeeId4, new DateOnly(2025, 1, 1)),
            new (_postId4, _employeeId2, new DateOnly(2025, 1, 1))
        };
        
        await _context.PostHistoryDb.AddRangeAsync(hierarchy);
        var hierarchy2 = new List<PositionHistoryDb>
        { new (_positionId1, _employeeId1, new DateOnly(2015, 1, 1)),
            new (_positionId2, _employeeId3, new DateOnly(2025, 1, 1)),
            new (_positionId3, _employeeId4, new DateOnly(2025, 1, 1)),
            new (_positionId4, _employeeId2, new DateOnly(2025, 1, 1))
        };
        
        await _context.PositionHistoryDb.AddRangeAsync(hierarchy2);
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSubordinatesPostHistoryAsync(
            _employeeId1, 
            pageNumber: 1, 
            pageSize: 4,
            startDate: filterStartDate,
            endDate: null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Items.Count);
        Assert.Equal(7, result.Page.TotalItems);
        Assert.Equal(2, result.Page.TotalPages);
    }
}