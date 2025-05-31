using Microsoft.Extensions.Logging;
using Moq;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Post;
using Project.Core.Repositories;
using Project.Services.PostService;
using Xunit;

namespace Project.Service.Tests;

public class PostServiceTests
{
    private readonly Mock<ILogger<PostService>> _mockLogger;
    private readonly Mock<IPostRepository> _mockRepository;
    private readonly PostService _postService;

    public PostServiceTests()
    {
        _mockRepository = new Mock<IPostRepository>();
        _mockLogger = new Mock<ILogger<PostService>>();
        _postService = new PostService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AddPost_Successful()
    {
        //Arrange
        var companyId = Guid.NewGuid();
        var postToAdd = new CreatePost(
            "Software Engineer",
            100000,
            companyId
        );

        var expectedPost = new BasePost(
            Guid.NewGuid(),
            "Software Engineer",
            100000,
            companyId
        );

        _mockRepository.Setup(x => x.AddPostAsync(It.IsAny<CreatePost>()))
            .ReturnsAsync(expectedPost);

        //Act
        var result = await _postService.AddPostAsync(
            "Software Engineer",
            100000,
            companyId);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPost.Id, result.Id);
        Assert.Equal(expectedPost.Title, result.Title);
        Assert.Equal(expectedPost.Salary, result.Salary);
        Assert.Equal(expectedPost.CompanyId, result.CompanyId);
        _mockRepository.Verify(x => x.AddPostAsync(It.IsAny<CreatePost>()), Times.Once);
    }

    [Fact]
    public async Task GetPostById_Successful()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var expectedPost = new BasePost(
            postId,
            "Software Engineer",
            100000,
            Guid.NewGuid()
        );

        _mockRepository.Setup(x => x.GetPostByIdAsync(postId))
            .ReturnsAsync(expectedPost);

        //Act
        var result = await _postService.GetPostByIdAsync(postId);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPost.Id, result.Id);
        Assert.Equal(expectedPost.Title, result.Title);
        _mockRepository.Verify(x => x.GetPostByIdAsync(postId), Times.Once);
    }

    [Fact]
    public async Task GetPostById_NotFound()
    {
        //Arrange
        var postId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetPostByIdAsync(postId))
            .ThrowsAsync(new PostNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<PostNotFoundException>(() =>
            _postService.GetPostByIdAsync(postId));
        _mockRepository.Verify(x => x.GetPostByIdAsync(postId), Times.Once);
    }

    [Fact]
    public async Task UpdatePost_Successful()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var updatePost = new UpdatePost(
            postId,
            companyId,
            "Senior Software Engineer",
            150000
        );

        var expectedPost = new BasePost(
            postId,
            updatePost.Title!,
            updatePost.Salary!.Value,
            companyId
        );

        _mockRepository.Setup(x => x.UpdatePostAsync(It.IsAny<UpdatePost>()))
            .ReturnsAsync(expectedPost);

        //Act
        var result = await _postService.UpdatePostAsync(
            postId,
            companyId,
            updatePost.Title,
            updatePost.Salary
        );

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPost.Title, result.Title);
        Assert.Equal(expectedPost.Salary, result.Salary);
        _mockRepository.Verify(x => x.UpdatePostAsync(It.IsAny<UpdatePost>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePost_NotFound()
    {
        //Arrange
        var postId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        _mockRepository.Setup(x => x.UpdatePostAsync(It.IsAny<UpdatePost>()))
            .ThrowsAsync(new PostNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<PostNotFoundException>(() =>
            _postService.UpdatePostAsync(postId, companyId, "New Title"));
        _mockRepository.Verify(x => x.UpdatePostAsync(It.IsAny<UpdatePost>()), Times.Once);
    }

    [Fact]
    public async Task GetPosts_Successful()
    {
        //Arrange
        var companyId = Guid.NewGuid();
        var pageNumber = 1;
        var pageSize = 10;
        var expectedPosts = new List<BasePost>
        {
            new(
                Guid.NewGuid(),
                "Software Engineer",
                100000,
                companyId
            ),
            new(
                Guid.NewGuid(),
                "Senior Software Engineer",
                150000,
                companyId
            )
        };

        var expectedPage = new PostPage(
            expectedPosts,
            new Page(pageNumber, 2, pageSize)
        );

        _mockRepository.Setup(x => x.GetPostsAsync(companyId, pageNumber, pageSize))
            .ReturnsAsync(expectedPage);

        //Act
        var result = await _postService.GetPostsByCompanyIdAsync(companyId, pageNumber, pageSize);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Page.TotalItems, result.Page.TotalItems);
        Assert.Equal(expectedPage.Posts.Count, result.Posts.Count);
        _mockRepository.Verify(x => x.GetPostsAsync(companyId, pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task DeletePost_Successful()
    {
        //Arrange
        var postId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeletePostAsync(postId))
            .Returns(Task.CompletedTask);

        //Act
        await _postService.DeletePostAsync(postId);

        //Assert
        _mockRepository.Verify(x => x.DeletePostAsync(postId), Times.Once);
    }

    [Fact]
    public async Task DeletePost_NotFound()
    {
        //Arrange
        var postId = Guid.NewGuid();
        _mockRepository.Setup(x => x.DeletePostAsync(postId))
            .ThrowsAsync(new PostNotFoundException());

        //Act & Assert
        await Assert.ThrowsAsync<PostNotFoundException>(() =>
            _postService.DeletePostAsync(postId));
        _mockRepository.Verify(x => x.DeletePostAsync(postId), Times.Once);
    }

    [Fact]
    public async Task AddPost_WithZeroSalary_ThrowsException()
    {
        //Arrange
        var companyId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _postService.AddPostAsync(
                "Software Engineer",
                0,
                companyId));

        _mockRepository.Verify(x => x.AddPostAsync(It.IsAny<CreatePost>()), Times.Never);
    }

    [Fact]
    public async Task AddPost_WithNegativeSalary_ThrowsException()
    {
        //Arrange
        var companyId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _postService.AddPostAsync(
                "Software Engineer",
                -100000,
                companyId));

        _mockRepository.Verify(x => x.AddPostAsync(It.IsAny<CreatePost>()), Times.Never);
    }

    [Fact]
    public async Task AddPost_WithEmptyTitle_ThrowsException()
    {
        //Arrange
        var companyId = Guid.NewGuid();

        //Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _postService.AddPostAsync(
                "",
                100000,
                companyId));

        _mockRepository.Verify(x => x.AddPostAsync(It.IsAny<CreatePost>()), Times.Never);
    }
}