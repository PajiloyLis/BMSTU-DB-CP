using Database.Context;
using Database.Models;
using Database.Repositories;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Position;
using Testcontainers.PostgreSql;
using Xunit;

namespace Project.Repository.Tests;

public class PositionRepositoryTests : IAsyncLifetime
{
    private readonly Mock<ILogger<PositionRepository>> _mockLogger;
    private readonly PostgreSqlContainer _postgresContainer;
    private ProjectDbContext _context;
    private PositionRepository _repository;

    public PositionRepositoryTests()
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

        _mockLogger = new Mock<ILogger<PositionRepository>>();

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

        _repository = new PositionRepository(_context, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
    
    [Fact]
    public async Task AddPositionAsync_ShouldAddPosition_WhenNoDuplicateExists()
    {
        // Arrange
        var company = new CompanyDb(
            Guid.NewGuid(),
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");
        var parentPosition = new PositionDb(Guid.NewGuid(), null, "Parent", company.Id);

        await _context.CompanyDb.AddAsync(company);
        await _context.PositionDb.AddAsync(parentPosition);
        await _context.SaveChangesAsync();
        
        var position = new CreatePosition(parentPosition.Id, "Senior Developer", company.Id);

        // Act
        var result = await _repository.AddPositionAsync(position);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Senior Developer", result.Title);
        Assert.Equal(company.Id, result.CompanyId);
        Assert.Equal(parentPosition.Id, result.ParentId);
        var dbRecord = await _context.PositionDb.FindAsync(result.Id);
        Assert.NotNull(dbRecord);
    }

    [Fact]
    public async Task AddPositionAsync_ShouldThrow_WhenDuplicateExists()
    {
        // Arrange
        var company = new CompanyDb(
            Guid.NewGuid(),
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");
        var parentPosition = new PositionDb(Guid.NewGuid(), null, "Parent", company.Id);

        await _context.CompanyDb.AddAsync(company);
        await _context.PositionDb.AddAsync(parentPosition);
        await _context.SaveChangesAsync();
        
        var position1 = new CreatePosition(parentPosition.Id, "Developer", company.Id);
        await _repository.AddPositionAsync(position1);

        var position2 = new CreatePosition(parentPosition.Id, "Developer", company.Id);

        // Act & Assert
        await Assert.ThrowsAsync<PositionAlreadyExistsException>(
            () => _repository.AddPositionAsync(position2));
    }

    [Fact]
    public async Task GetPositionByIdAsync_ShouldReturnPosition_WhenExists()
    {
        // Arrange
        var company = new CompanyDb(
            Guid.NewGuid(),
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");
        var parentPosition = new PositionDb(Guid.NewGuid(), null, "Parent", company.Id);

        await _context.CompanyDb.AddAsync(company);
        await _context.PositionDb.AddAsync(parentPosition);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _repository.GetPositionByIdAsync(parentPosition.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parentPosition.Id, result.Id);
        Assert.Equal("Parent", result.Title);
    }

    [Fact]
    public async Task GetPositionByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<PositionNotFoundException>(
            () => _repository.GetPositionByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdatePositionAsync_ShouldUpdateFields_WhenValid()
    {
        // Arrange
        var company = new CompanyDb(
            Guid.NewGuid(),
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");
        var parentPosition = new PositionDb(Guid.NewGuid(), null, "Parent", company.Id);

        await _context.CompanyDb.AddAsync(company);
        await _context.PositionDb.AddAsync(parentPosition);
        await _context.SaveChangesAsync();
        
        var position = new CreatePosition(parentPosition.Id, "Junior Developer", company.Id);
        var addedPosition = await _repository.AddPositionAsync(position);

        var update = new UpdatePosition(
            addedPosition.Id,
            company.Id,
            parentPosition.Id,
            "Senior Developer");

        // Act
        var result = await _repository.UpdatePositionAsync(update);

        // Assert
        Assert.Equal("Senior Developer", result.Title);
        
        var dbRecord = await _context.PositionDb.FindAsync(addedPosition.Id);
        Assert.Equal("Senior Developer", dbRecord!.Title);
    }

    [Fact]
    public async Task DeletePositionAsync_ShouldRemovePosition_WhenExists()
    {
        // Arrange
        var company = new CompanyDb(
            Guid.NewGuid(),
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");
        var parentPosition = new PositionDb(Guid.NewGuid(), null, "Parent", company.Id);

        await _context.CompanyDb.AddAsync(company);
        await _context.PositionDb.AddAsync(parentPosition);
        await _context.SaveChangesAsync();
        
        var position = new CreatePosition(parentPosition.Id, "CEO", company.Id);
        var addedPosition = await _repository.AddPositionAsync(position);

        // Act
        await _repository.DeletePositionAsync(addedPosition.Id);

        // Assert
        await Assert.ThrowsAsync<PositionNotFoundException>(
            () => _repository.GetPositionByIdAsync(addedPosition.Id));
    }

    [Fact]
    public async Task GetSubordinatesAsync_ShouldReturnHierarchy()
    {
        // Arrange
        var company = new CompanyDb(
            Guid.NewGuid(),
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");

        await _context.CompanyDb.AddAsync(company);
        await _context.SaveChangesAsync();
        
        // Создаем иерархию должностей:
        // CEO
        // ├── CTO
        // │   ├── Team Lead
        // │   └── Architect
        // └── CFO
        
        var ceo = await _repository.AddPositionAsync(new CreatePosition(null, "CEO", company.Id));
        var cto = await _repository.AddPositionAsync(new CreatePosition(ceo.Id,"CTO", company.Id));
        var cfo = await _repository.AddPositionAsync(new CreatePosition(ceo.Id, "CFO", company.Id));
        var teamLead = await _repository.AddPositionAsync(new CreatePosition(cto.Id, "Team Lead", company.Id));
        var architect = await _repository.AddPositionAsync(new CreatePosition(cto.Id, "Architect", company.Id));

        // Act
        var result = await _repository.GetSubordinatesAsync(ceo.Id, 1, 10);

        // Assert
        Assert.Equal(5, result.Page.TotalItems); // CEO, CTO, CFO, Team Lead, Architect
        Assert.Contains(result.Items, p => p.Title == "CTO" && p.Level == 1);
        Assert.Contains(result.Items, p => p.Title == "Team Lead" && p.Level == 2);
        Assert.Contains(result.Items, p => p.Title == "Architect" && p.Level == 2);
        Assert.Contains(result.Items, p => p.Title == "CFO" && p.Level == 1);
        Assert.Contains(result.Items, p => p.Title == "CEO" && p.Level == 0);
    }

    [Fact]
    public async Task GetSubordinatesAsync_ShouldPaginateResults()
    {
        // Arrange
        var company = new CompanyDb(
            Guid.NewGuid(),
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");
        var parentPosition = new PositionDb(Guid.NewGuid(), null, "Parent", company.Id);

        await _context.CompanyDb.AddAsync(company);
        await _context.PositionDb.AddAsync(parentPosition);
        await _context.SaveChangesAsync();
        
        // Создаем 15 подчиненных должностей
        for (int i = 0; i < 15; i++)
        {
            await _repository.AddPositionAsync(
                new CreatePosition(parentPosition.Id, $"Position {i}", company.Id));
        }

        // Act
        var page1 = await _repository.GetSubordinatesAsync(parentPosition.Id, 1, 5);
        var page2 = await _repository.GetSubordinatesAsync(parentPosition.Id, 2, 5);

        // Assert
        Assert.Equal(5, page1.Items.Count);
        Assert.Equal(5, page2.Items.Count);
        Assert.Equal(16, page1.Page.TotalItems);
        Assert.Equal(4, page1.Page.TotalPages);
    }
}