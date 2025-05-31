using Database.Context;
using Database.Models;
using Database.Repositories;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Education;
using Testcontainers.PostgreSql;
using Xunit;

namespace Project.Repository.Tests;

public class EducationRepositoryTests : IAsyncLifetime
{
    private readonly Mock<ILogger<EducationRepository>> _mockLogger;
    private readonly PostgreSqlContainer _postgresContainer;
    private ProjectDbContext _context;
    private EducationRepository _repository;

    public EducationRepositoryTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            // .WithPortBinding(5432, assignRandomHostPort: false)
            .WithDatabase("ppo_test")
            .WithUsername("test")
            .WithPassword("test")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5432))
            .Build();

        _mockLogger = new Mock<ILogger<EducationRepository>>();

    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString()).EnableSensitiveDataLogging()  // Показывает значения параметров
            .LogTo(Console.WriteLine, LogLevel.Information)
            .Options;

        _context = new ProjectDbContext(options);
        // await Task.Delay(2);

        await _context.Database.MigrateAsync();
        // await Task.Delay(2);
        
        _repository = new EducationRepository(_context, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
    
    [Fact]
    public async Task AddEducationAsync_ShouldAddEducation_WhenNoDuplicateExists()
    {
        // Arrange
        var employee = new EmployeeDb(
            Guid.NewGuid(),
            "Иванов Иван Иванович",
            "+77777777777",
            "kek@gmail.com",
            new DateOnly(2003, 9, 22),
            null, null
        );
        await _context.EmployeeDb.AddAsync(employee);
        await _context.SaveChangesAsync();
        var education = new CreateEducation(
            employee.Id,
            "MIT",
            "Высшее (бакалавриат)",
            "Computer Science",
            new DateOnly(2020, 9, 1),
             new DateOnly(2024, 6, 30)
        );


        // Act
        var result = await _repository.AddEducationAsync(education);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.EmployeeId);

        var dbRecord = await _context.EducationDb.FindAsync(result.Id);
        Assert.NotNull(dbRecord);
        Assert.Equal("MIT", dbRecord.Institution);
    }

    [Fact]
    public async Task AddEducationAsync_ShouldThrow_WhenDuplicateExists()
    {
        // Arrange
        var employee = new EmployeeDb(
            Guid.NewGuid(),
            "Иванов Иван Иванович",
            "+77777777777",
            "kek@gmail.com",
            new DateOnly(2003, 9, 22),
            null, null
        );
        await _context.EmployeeDb.AddAsync(employee);
        await _context.SaveChangesAsync();
        var education = new CreateEducation(employee.Id, "MIT", "Высшее (бакалавриат)", "CS", DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), null);
        await _repository.AddEducationAsync(education);

        // Act & Assert
        await Assert.ThrowsAsync<EducationAlreadyExistsException>(() => 
            _repository.AddEducationAsync(education));
    }

    [Fact]
    public async Task GetEducationByIdAsync_ShouldReturnEducation_WhenExists()
    {
        // Arrange
        var employee = new EmployeeDb(
            Guid.NewGuid(),
            "Иванов Иван Иванович",
            "+77777777777",
            "kek@gmail.com",
            new DateOnly(2003, 9, 22),
            null, null
        );
        await _context.EmployeeDb.AddAsync(employee);
        await _context.SaveChangesAsync();
        var education = await _repository.AddEducationAsync(
            new CreateEducation(employee.Id, "Harvard", "Высшее (магистратура)", "Law",  new DateOnly(2024, 05, 26),null));

        // Act
        var result = await _repository.GetEducationByIdAsync(education.Id);

        // Assert
        Assert.Equal(education.EmployeeId, result.EmployeeId);
        Assert.Equal(education.Level, result.Level);
        Assert.Equal(education.Institution, result.Institution);
        Assert.Equal(education.StudyField, result.StudyField);
        Assert.Equal(education.StartDate, result.StartDate);
        Assert.Equal(education.EndDate, result.EndDate);
    }

    [Fact]
    public async Task GetEducationByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<EducationNotFoundException>(() => 
            _repository.GetEducationByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateEducationAsync_ShouldUpdateFields_WhenValid()
    {
        // Arrange
        var employee = new EmployeeDb(
            Guid.NewGuid(),
            "Иванов Иван Иванович",
            "+77777777777",
            "kek@gmail.com",
            new DateOnly(2003, 9, 22),
            null, null
        );
        await _context.EmployeeDb.AddAsync(employee);
        await _context.SaveChangesAsync();
        var original = await _repository.AddEducationAsync(
            new CreateEducation(employee.Id, "Stanford", "Высшее (бакалавриат)", "Medicine",  new DateOnly(2024, 9, 1), null));

        var update = new UpdateEducation(
            original.Id,
            original.EmployeeId,
            "Stanford Updated",
            "Высшее (магистратура)",
             "Biomedicine",
            original.StartDate.AddMonths(1),
             null
        );

        // Act
        var result = await _repository.UpdateEducationAsync(update);

        // Assert
        Assert.Equal("Stanford Updated", result.Institution);
        Assert.Equal("Biomedicine", result.StudyField);
        Assert.Equal(EducationLevel.Master, result.Level);
    }

    [Fact]
    public async Task GetEducationsAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        var employee = new EmployeeDb(
            Guid.NewGuid(),
            "Иванов Иван Иванович",
            "+77777777777",
            "kek@gmail.com",
            new DateOnly(2003, 9, 22),
            null, null
        );
        await _context.EmployeeDb.AddAsync(employee);
        await _context.SaveChangesAsync();
        for (int i = 0; i < 10; i++)
        {
            await _repository.AddEducationAsync(
                new CreateEducation(employee.Id, $"University {i}", "Высшее (бакалавриат)", $"Field {i}",  new DateOnly(2024, 9, 1), null));
        }

        // Act
        var page1 = await _repository.GetEducationsAsync(employee.Id, pageNumber: 1, pageSize: 3);
        var page2 = await _repository.GetEducationsAsync(employee.Id, pageNumber: 2, pageSize: 3);

        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.Equal(4, page1.Page.TotalPages);
        Assert.Equal(3, page1.Educations.Count);
        Assert.Equal(3, page2.Educations.Count);
        Assert.Equal(4, page2.Page.TotalPages);
        Assert.Equal(1, page1.Page.PageNumber);
        Assert.Equal(2, page2.Page.PageNumber);
    }

    [Fact]
    public async Task DeleteEducationAsync_ShouldRemoveRecord_WhenExists()
    {
        // Arrange
        var employee = new EmployeeDb(
            Guid.NewGuid(),
            "Иванов Иван Иванович",
            "+77777777777",
            "kek@gmail.com",
            new DateOnly(2003, 9, 22),
            null, null
        );
        await _context.EmployeeDb.AddAsync(employee);
        await _context.SaveChangesAsync();
        var education = await _repository.AddEducationAsync(
            new CreateEducation(employee.Id, "Oxford", "Высшее (бакалавриат)", "History", new DateOnly(2020, 9, 1), new DateOnly(2020, 9, 1).AddYears(4)));

        // Act
        await _repository.DeleteEducationAsync(education.Id);

        // Assert
        await Assert.ThrowsAsync<EducationNotFoundException>(() => 
            _repository.GetEducationByIdAsync(education.Id));
    }

}