using Database.Context;
using Database.Models;
using Database.Repositories;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Company;
using Testcontainers.PostgreSql;
using Xunit;

namespace Project.Repository.Tests;

public class CompanyRepositoryTests : IAsyncLifetime
{
    private readonly Mock<ILogger<CompanyRepository>> _mockLogger;
    private readonly PostgreSqlContainer _postgresContainer;
    private ProjectDbContext _context;
    private CompanyRepository _repository;

    public CompanyRepositoryTests()
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

        _mockLogger = new Mock<ILogger<CompanyRepository>>();

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

        _repository = new CompanyRepository(_context, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
    
    [Fact]
    public async Task AddCompanyAsync_ShouldAddNewCompany()
    {
        // Arrange
        var newCompany = new CreationCompany(
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");

        // Act
        var result = await _repository.AddCompanyAsync(newCompany);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.CompanyId);
        Assert.Equal(newCompany.Title, result.Title);

        var dbCompany = await _context.CompanyDb.FirstOrDefaultAsync(c => c.Id == result.CompanyId);
        Assert.NotNull(dbCompany);
    }

    [Fact]
    public async Task AddCompanyAsync_ShouldThrowWhenCompanyExists()
    {
        // Arrange
        var existingCompany = new CompanyDb
        (
            Guid.NewGuid(),
            "Existing Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+1234567890",
             "existing@example.com",
             "1234567890",
             "123456789",
             "1234567890123",
             "Existing Address"
            );

        await _context.CompanyDb.AddAsync(existingCompany);
        await _context.SaveChangesAsync();

        var duplicateCompany = new CreationCompany(
            existingCompany.Title,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            existingCompany.PhoneNumber,
            existingCompany.Email,
            existingCompany.Inn,
            existingCompany.Kpp,
            existingCompany.Ogrn,
            "New Address");

        // Act & Assert
        await Assert.ThrowsAsync<CompanyAlreadyExistsException>(() => 
            _repository.AddCompanyAsync(duplicateCompany));
    }

    [Fact]
    public async Task UpdateCompanyAsync_ShouldUpdateExistingCompany()
    {
        // Arrange
        var existingCompany = new CompanyDb
        (
             Guid.NewGuid(),
             "Original Company",
             DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
             "+1111111111",
             "original@example.com",
             "1111111111",
             "111111111",
             "1111111111111",
             "Original Address"
        );

        await _context.CompanyDb.AddAsync(existingCompany);
        await _context.SaveChangesAsync();

        var updateModel = new UpdateCompany(
            existingCompany.Id,
            "Updated Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+2222222222",
            "updated@example.com",
            "2222222222",
            "222222222",
            "2222222222222",
            "Updated Address"
        );

        // Act
        var result = await _repository.UpdateCompanyAsync(updateModel);

        // Assert
        Assert.Equal(updateModel.CompanyId, result.CompanyId);
        Assert.Equal(updateModel.Title, result.Title);
        Assert.Equal(updateModel.PhoneNumber, result.PhoneNumber);

        var updatedCompany = await _context.CompanyDb.FirstOrDefaultAsync(c => c.Id == existingCompany.Id);
        Assert.Equal(updateModel.Title, updatedCompany.Title);
    }

    [Fact]
    public async Task UpdateCompanyAsync_ShouldThrowWhenCompanyNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateModel = new UpdateCompany(
            nonExistentId,
            "Non-existent Company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            "+0000000000",
            "nonexistent@example.com",
            "0000000000",
            "000000000",
            "0000000000000",
            "Non-existent Address"
            );

        // Act & Assert
        await Assert.ThrowsAsync<CompanyNotFoundException>(() => 
            _repository.UpdateCompanyAsync(updateModel));
    }

    [Fact]
    public async Task GetCompanyByIdAsync_ShouldReturnCompany()
    {
        // Arrange
        var expectedCompany = new CompanyDb
        (
             Guid.NewGuid(),
             "Test Company",
             DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
             "+1234567890",
             "test@example.com",
             "1234567890",
             "123456789",
             "1234567890123",
             "Test Address"
        );

        await _context.CompanyDb.AddAsync(expectedCompany);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCompanyByIdAsync(expectedCompany.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCompany.Id, result.CompanyId);
        Assert.Equal(expectedCompany.Title, result.Title);
    }

    [Fact]
    public async Task GetCompanyByIdAsync_ShouldThrowWhenCompanyNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<CompanyNotFoundException>(() => 
            _repository.GetCompanyByIdAsync(nonExistentId));
    }

    [Fact]
    public async Task DeleteCompanyAsync_ShouldRemoveCompany()
    {
        // Arrange
        var companyToDelete = new CompanyDb
        (
             Guid.NewGuid(),
             "Company to Delete",
             DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
             "+1234567890",
             "delete@example.com",
             "1234567890",
             "123456789",
             "1234567890123",
             "Delete Address"
        );

        await _context.CompanyDb.AddAsync(companyToDelete);
        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();
        
        // Act
        await _repository.DeleteCompanyAsync(companyToDelete.Id);

        // Assert
        var deletedCompany = await _context.CompanyDb.AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyToDelete.Id);
        Assert.Null(deletedCompany);
    }

    [Fact]
    public async Task DeleteCompanyAsync_ShouldNotThrowWhenCompanyNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var exception = await Record.ExceptionAsync(() => 
            _repository.DeleteCompanyAsync(nonExistentId));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task GetCompaniesAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var companies = new List<CompanyDb>();
        for (int i = 0; i < 10; i++)
        {
            companies.Add(new CompanyDb
            (
                Guid.NewGuid(),
                $"Company {i}",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)),
                $"+{i}234567890",
                $"company{i}@example.com",
                 $"{i}234567890",
                 $"{i}23456789",
                 $"{i}234567890123",
                 $"Address {i}"
            ));
        }

        await _context.CompanyDb.AddRangeAsync(companies);
        await _context.SaveChangesAsync();

        // Act
        var page1 = await _repository.GetCompaniesAsync(1, 5);
        var page2 = await _repository.GetCompaniesAsync(2, 5);

        // Assert
        Assert.Equal(5, page1.Companies.Count);
        Assert.Equal(5, page1.Page.TotalItems);
        Assert.Equal(1, page1.Page.PageNumber);

        Assert.Equal(5, page2.Companies.Count);
        Assert.Equal(5, page2.Page.TotalItems);
        Assert.Equal(2, page2.Page.PageNumber);

        Assert.NotEqual(page1.Companies.First().CompanyId, page2.Companies.First().CompanyId);
    }
}