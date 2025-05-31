using Database.Context;
using Database.Models.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Post;
using Project.Core.Repositories;

namespace Database.Repositories;

public class PostRepository : IPostRepository
{
    private readonly ProjectDbContext _context;
    private readonly ILogger<PostRepository> _logger;

    public PostRepository(ProjectDbContext context, ILogger<PostRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BasePost> AddPostAsync(CreatePost post)
    {
        try
        {
            var postDb = PostConverter.Convert(post);
            if (postDb is null)
            {
                _logger.LogWarning("Failed to convert CreatePost to PostDb");
                throw new ArgumentException("Failed to convert CreatePost to PostDb");
            }

            var existingPost = await _context.PostDb
                .FirstOrDefaultAsync(p => p.CompanyId == post.CompanyId && p.Title == post.Title);

            if (existingPost is not null)
            {
                _logger.LogWarning("Post with title {Title} already exists in company {CompanyId}", post.Title,
                    post.CompanyId);
                throw new PostAlreadyExistsException(
                    $"Post with title {post.Title} already exists in company {post.CompanyId}");
            }

            await _context.PostDb.AddAsync(postDb);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Post with id {Id} was added", postDb.Id);
            return PostConverter.Convert(postDb)!;
        }
        catch (Exception e) when (e is not PostAlreadyExistsException)
        {
            _logger.LogError(e, "Error occurred while adding post");
            throw;
        }
    }

    public async Task<BasePost> GetPostByIdAsync(Guid postId)
    {
        try
        {
            var post = await _context.PostDb
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post is null)
            {
                _logger.LogWarning("Post with id {Id} not found", postId);
                throw new PostNotFoundException($"Post with id {postId} not found");
            }

            _logger.LogInformation("Post with id {Id} was retrieved", postId);
            return PostConverter.Convert(post)!;
        }
        catch (Exception e) when (e is not PostNotFoundException)
        {
            _logger.LogError(e, "Error occurred while getting post with id {Id}", postId);
            throw;
        }
    }

    public async Task<BasePost> UpdatePostAsync(UpdatePost post)
    {
        try
        {
            var postDb = await _context.PostDb
                .FirstOrDefaultAsync(p => p.Id == post.Id && p.CompanyId == post.CompanyId);

            if (postDb is null)
            {
                _logger.LogWarning("Post with id {Id} not found for update", post.Id);
                throw new PostNotFoundException($"Post with id {post.Id} not found");
            }

            var existingPost = await _context.PostDb
                .Where(p => p.Id != post.Id &&
                            p.CompanyId == post.CompanyId &&
                            p.Title == post.Title)
                .FirstOrDefaultAsync();

            if (existingPost is not null)
            {
                _logger.LogWarning("Post with title {Title} already exists in company {CompanyId}", post.Title,
                    post.CompanyId);
                throw new PostAlreadyExistsException(
                    $"Post with title {post.Title} already exists in company {post.CompanyId}");
            }

            postDb.Title = post.Title ?? postDb.Title;
            postDb.Salary = post.Salary ?? postDb.Salary;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Post with id {Id} was updated", post.Id);
            return PostConverter.Convert(postDb)!;
        }
        catch (Exception e) when (e is not PostNotFoundException and not PostAlreadyExistsException)
        {
            _logger.LogError(e, "Error occurred while updating post with id {Id}", post.Id);
            throw;
        }
    }

    public async Task<PostPage> GetPostsAsync(Guid companyId, int pageNumber, int pageSize)
    {
        try
        {
            var posts = await _context.PostDb
                .Where(p => p.CompanyId == companyId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await _context.PostDb
                .CountAsync(p => p.CompanyId == companyId);

            var page = new Page(pageNumber, (int)Math.Ceiling(totalItems/(double)pageSize), totalItems);
            var result = new PostPage(posts.Select(p => PostConverter.Convert(p)!).ToList(), page);

            _logger.LogInformation("Posts for company {CompanyId} were retrieved", companyId);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while getting posts for company {CompanyId}", companyId);
            throw;
        }
    }

    public async Task DeletePostAsync(Guid postId)
    {
        try
        {
            var post = await _context.PostDb
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post is null)
            {
                _logger.LogWarning("Post with id {Id} not found for deletion", postId);
                throw new PostNotFoundException($"Post with id {postId} not found");
            }

            _context.PostDb.Remove(post);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Post with id {Id} was deleted", postId);
        }
        catch (Exception e) when (e is not PostNotFoundException)
        {
            _logger.LogError(e, "Error occurred while deleting post with id {Id}", postId);
            throw;
        }
    }
}