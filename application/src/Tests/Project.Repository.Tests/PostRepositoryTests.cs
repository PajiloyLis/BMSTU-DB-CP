using Database.Context;
using Database.Models;
using Database.Repositories;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Post;
using Testcontainers.PostgreSql;
using Xunit;

namespace Project.Repository.Tests;

public class PostRepositoryTests : IAsyncLifetime
{
    private readonly Mock<ILogger<PostRepository>> _mockLogger;
    private readonly PostgreSqlContainer _postgresContainer;

    private Guid _companyId;
    private ProjectDbContext _context;
    private PostRepository _repository;

    public PostRepositoryTests()
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

        _mockLogger = new Mock<ILogger<PostRepository>>();
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

        var company = new CompanyDb(
            Guid.NewGuid(),
            "Test Company",
            DateOnly.FromDateTime(DateTime.UtcNow),
            "+1234567890",
            "test@example.com",
            "1234567890",
            "123456789",
            "1234567890123",
            "Test Address");

        await _context.CompanyDb.AddAsync(company);
        await _context.SaveChangesAsync();
        _companyId = company.Id;

        _repository = new PostRepository(_context, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task AddPostAsync_ShouldAddPost_WhenNoDuplicateExists()
    {
        // Arrange
        var post = new CreatePost("Senior Developer", 100000, _companyId);

        // Act
        var result = await _repository.AddPostAsync(post);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Senior Developer", result.Title);
        Assert.Equal(100000, result.Salary);
        Assert.Equal(_companyId, result.CompanyId);

        var dbRecord = await _context.PostDb.FindAsync(result.Id);
        Assert.NotNull(dbRecord);
    }

    [Fact]
    public async Task AddPostAsync_ShouldThrow_WhenDuplicateTitleInSameCompany()
    {
        // Arrange
        var post1 = new CreatePost("Developer", 80000, _companyId);
        await _repository.AddPostAsync(post1);

        var post2 = new CreatePost("Developer", 85000, _companyId);

        // Act & Assert
        await Assert.ThrowsAsync<PostAlreadyExistsException>(() => _repository.AddPostAsync(post2));
    }

    [Fact]
    public async Task AddPostAsync_ShouldAllowSameTitle_InDifferentCompanies()
    {
        // Arrange
        var company2 = new CompanyDb(Guid.NewGuid(), "Рога и копыта", new DateOnly(1999, 12, 12), "+79999999999",
            "rogaikopyta@mail.ru", "1234567891", "987654321", "1234567891234", "some address");
        await _context.CompanyDb.AddAsync(company2);
        await _context.SaveChangesAsync();

        await _repository.AddPostAsync(new CreatePost("Manager", 90000, _companyId));

        // Act
        var result = await _repository.AddPostAsync(new CreatePost("Manager", 95000, company2.Id));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(company2.Id, result.CompanyId);
    }

    [Fact]
    public async Task GetPostByIdAsync_ShouldReturnPost_WhenExists()
    {
        // Arrange
        var post = new CreatePost("CTO", 150000, _companyId);
        var addedPost = await _repository.AddPostAsync(post);

        // Act
        var result = await _repository.GetPostByIdAsync(addedPost.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(addedPost.Id, result.Id);
        Assert.Equal("CTO", result.Title);
    }

    [Fact]
    public async Task GetPostByIdAsync_ShouldThrow_WhenNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<PostNotFoundException>(() => _repository.GetPostByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdatePostAsync_ShouldUpdateFields_WhenValid()
    {
        // Arrange
        var post = new CreatePost("Junior Developer", 60000, _companyId);
        var addedPost = await _repository.AddPostAsync(post);

        var update = new UpdatePost(
            addedPost.Id,
            _companyId,
            "Senior Developer",
            90000);

        // Act
        var result = await _repository.UpdatePostAsync(update);

        // Assert
        Assert.Equal("Senior Developer", result.Title);
        Assert.Equal(90000, result.Salary);

        var dbRecord = await _context.PostDb.FindAsync(addedPost.Id);
        Assert.Equal("Senior Developer", dbRecord!.Title);
        Assert.Equal(90000, dbRecord.Salary);
    }

    [Fact]
    public async Task UpdatePostAsync_ShouldThrow_WhenDuplicateTitleInSameCompany()
    {
        // Arrange
        var post1 = await _repository.AddPostAsync(new CreatePost("Designer", 70000, _companyId));
        var post2 = await _repository.AddPostAsync(new CreatePost("Developer", 80000, _companyId));

        var update = new UpdatePost(post2.Id, _companyId, "Designer", 85000);

        // Act & Assert
        await Assert.ThrowsAsync<PostAlreadyExistsException>(() => _repository.UpdatePostAsync(update));
    }

    [Fact]
    public async Task GetPostsAsync_ShouldReturnPaginatedResults()
    {
        // Arrange
        // Создаем 15 должностей
        for (var i = 0; i < 15; i++)
            await _repository.AddPostAsync(
                new CreatePost($"Position {i}", 50000 + i * 1000, _companyId));

        // Act
        var page1 = await _repository.GetPostsAsync(_companyId, 1, 5);
        var page2 = await _repository.GetPostsAsync(_companyId, 2, 5);

        // Assert
        Assert.Equal(5, page1.Posts.Count);
        Assert.Equal(5, page2.Posts.Count);
        Assert.Equal(15, page1.Page.TotalItems);
        Assert.Equal(3, page1.Page.TotalPages);
    }

    [Fact]
    public async Task DeletePostAsync_ShouldRemovePost_WhenExists()
    {
        // Arrange
        var post = new CreatePost("CEO", 200000, _companyId);
        var addedPost = await _repository.AddPostAsync(post);

        // Act
        await _repository.DeletePostAsync(addedPost.Id);

        // Assert
        await Assert.ThrowsAsync<PostNotFoundException>(() => _repository.GetPostByIdAsync(addedPost.Id));
    }

    [Fact]
    public async Task DeletePostAsync_ShouldThrow_WhenNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<PostNotFoundException>(() => _repository.DeletePostAsync(Guid.NewGuid()));
    }
}