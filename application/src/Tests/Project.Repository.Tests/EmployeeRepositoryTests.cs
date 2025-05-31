using System.ComponentModel;
using Database.Context;
using Database.Repositories;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Employee;
using Testcontainers.PostgreSql;
using Xunit;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Project.Repository.Tests;

public class EmployeeRepositoryTests : IAsyncLifetime
{
    private readonly Mock<ILogger<EmployeeRepository>> _mockLogger;
    private readonly PostgreSqlContainer _postgresContainer;
    private ProjectDbContext _context;
    private EmployeeRepository _repository;

    public EmployeeRepositoryTests()
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

        _mockLogger = new Mock<ILogger<EmployeeRepository>>();
        
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

        _repository = new EmployeeRepository(_context, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddEmployee_Successful()
    {
        // Arrange
        var employeeToAdd = new CreationEmployee(
            "John Doe",
            "+1234567890",
            "john.doe@example.com",
            new DateOnly(1990, 1, 1),
            "photo.jpg",
            "{\"Developer\": true}"
        );

        // Act
        var result = await _repository.AddEmployeeAsync(employeeToAdd);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeToAdd.FullName, result.FullName);
        Assert.Equal(employeeToAdd.Email, result.Email);
        Assert.Equal(employeeToAdd.PhoneNumber, result.PhoneNumber);
        Assert.Equal(employeeToAdd.BirthDate, result.BirthDate);
        Assert.Equal(employeeToAdd.Duties, result.Duties);
        Assert.Equal(employeeToAdd.Photo, result.Photo);

        var savedEmployee = await _context.EmployeeDb.FirstOrDefaultAsync(e => e.Id == result.EmployeeId);
        Assert.NotNull(savedEmployee);
        Assert.Equal(employeeToAdd.FullName, savedEmployee.FullName);
    }

    [Fact]
    public async Task AddEmployee_AlreadyExists_ThrowsException()
    {
        // Arrange
        var employeeToAdd = new CreationEmployee(
            "John Doe",
            "+1234567890",
            "john.doe@example.com",
            new DateOnly(1990, 1, 1),
            "photo.jpg",
            "{\"Developer\": true}"
        );

        await _repository.AddEmployeeAsync(employeeToAdd);

        // Act & Assert
        await Assert.ThrowsAsync<EmployeeAlreadyExistsException>(() =>
            _repository.AddEmployeeAsync(employeeToAdd));
    }

    [Fact]
    public async Task GetEmployeeById_Successful()
    {
        // Arrange
        var employeeToAdd = new CreationEmployee(
            "John Doe",
            "+1234567890",
            "john.doe@example.com",
            new DateOnly(1990, 1, 1),
            "photo.jpg",
            "{\"Developer\": true}"
        );

        var addedEmployee = await _repository.AddEmployeeAsync(employeeToAdd);

        // Act
        var result = await _repository.GetEmployeeByIdAsync(addedEmployee.EmployeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedEmployee.EmployeeId, result.EmployeeId);
        Assert.Equal(addedEmployee.FullName, result.FullName);
        Assert.Equal(addedEmployee.Email, result.Email);
        Assert.Equal(addedEmployee.PhoneNumber, result.PhoneNumber);
        Assert.Equal(addedEmployee.BirthDate, result.BirthDate);
        Assert.Equal(addedEmployee.Duties, result.Duties);
        Assert.Equal(addedEmployee.Photo, result.Photo);
    }

    [Fact]
    public async Task GetEmployeeById_NotFound_ThrowsException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EmployeeNotFoundException>(() =>
            _repository.GetEmployeeByIdAsync(nonExistentId));
    }

    [Fact]
    public async Task UpdateEmployee_Successful()
    {
        // Arrange
        var employeeToAdd = new CreationEmployee(
            "John Doe",
            "+1234567890",
            "john.doe@example.com",
            new DateOnly(1990, 1, 1),
            "photo.jpg",
            "{\"Developer\": true}"
        );

        var addedEmployee = await _repository.AddEmployeeAsync(employeeToAdd);

        var updateEmployee = new UpdateEmployee(
            addedEmployee.EmployeeId,
            "Jane Doe",
            "+0987654321",
            "jane.doe@example.com",
            new DateOnly(1991, 2, 2),
            "new-photo.jpg",
            "{\"Manager\": true}"
        );

        // Act
        var result = await _repository.UpdateEmployeeAsync(updateEmployee);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateEmployee.FullName, result.FullName);
        Assert.Equal(updateEmployee.Email, result.Email);
        Assert.Equal(updateEmployee.PhoneNumber, result.PhoneNumber);
        Assert.Equal(updateEmployee.BirthDate, result.BirthDate);
        Assert.Equal(updateEmployee.Duties, result.Duties);
        Assert.Equal(updateEmployee.Photo, result.Photo);

        var updatedEmployee = await _context.EmployeeDb.FirstOrDefaultAsync(e => e.Id == result.EmployeeId);
        Assert.NotNull(updatedEmployee);
        Assert.Equal(updateEmployee.FullName, updatedEmployee.FullName);
    }

    [Fact]
    public async Task UpdateEmployee_NotFound_ThrowsException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateEmployee = new UpdateEmployee(
            nonExistentId,
            "Jane Doe",
            "+0987654321",
            "jane.doe@example.com",
            new DateOnly(1991, 2, 2),
            "new-photo.jpg",
            "{\"Manager\": true}"
        );

        // Act & Assert
        await Assert.ThrowsAsync<EmployeeNotFoundException>(() =>
            _repository.UpdateEmployeeAsync(updateEmployee));
    }

    [Fact]
    public async Task DeleteEmployee_Successful()
    {
        // Arrange
        var employeeToAdd = new CreationEmployee(
            "John Doe",
            "+1234567890",
            "john.doe@example.com",
            new DateOnly(1990, 1, 1),
            "photo.jpg",
            "{\"Developer\": true}"
        );

        var addedEmployee = await _repository.AddEmployeeAsync(employeeToAdd);

        // Act
        await _repository.DeleteEmployeeAsync(addedEmployee.EmployeeId);

        // Assert
        var deletedEmployee = await _context.EmployeeDb.FirstOrDefaultAsync(e => e.Id == addedEmployee.EmployeeId);
        Assert.Null(deletedEmployee);
    }

    [Fact]
    public async Task DeleteEmployee_NotFound_ThrowsException()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EmployeeNotFoundException>(() =>
            _repository.DeleteEmployeeAsync(nonExistentId));
    }
}