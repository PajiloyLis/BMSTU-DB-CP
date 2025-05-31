using System.Text.Json;
using Microsoft.Extensions.Logging;
using Project.Core.Models;
using Project.Core.Models.Post;
using Project.Core.Repositories;
using Project.Core.Services;
using StackExchange.Redis;

namespace Project.Services.PostService;

public class PostService : IPostService
{
    public static bool CacheDirty;
    private readonly IDatabaseAsync _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<PostService> _logger;
    private readonly IPostRepository _postRepository;

    public PostService(IPostRepository postRepository, ILogger<PostService> logger,
        IConnectionMultiplexer connectionMultiplexer)
    {
        _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionMultiplexer =
            connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _cache = _connectionMultiplexer.GetDatabase();
    }

    public async Task<BasePost> AddPostAsync(string title, decimal salary, Guid companyId)
    {
        try
        {
            var post = new CreatePost(title, salary, companyId);
            var result = await _postRepository.AddPostAsync(post);
            await _cache.StringSetAsync($"post_{result.Id}", JsonSerializer.Serialize(result), TimeSpan.FromMinutes(5));
            CacheDirty = true;
            _logger.LogInformation("Post with id {Id} was added", result.Id);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while adding post");
            throw;
        }
    }

    public async Task<BasePost> GetPostByIdAsync(Guid postId)
    {
        try
        {
            if (!CacheDirty)
            {
                var cachedData = await _cache.StringGetAsync($"post_{postId}");
                if (cachedData.HasValue)
                    return JsonSerializer.Deserialize<BasePost>(cachedData);
            }
            else
            {
                await DeleteCache();
                CacheDirty = false;
            }

            var result = await _postRepository.GetPostByIdAsync(postId);

            await _cache.StringSetAsync($"post_{result.Id}", JsonSerializer.Serialize(result), TimeSpan.FromMinutes(5));

            _logger.LogInformation("Post with id {Id} was retrieved", postId);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while getting post with id {Id}", postId);
            throw;
        }
    }

    public async Task<BasePost> UpdatePostAsync(Guid postId, Guid companyId, string? title = null,
        decimal? salary = null)
    {
        try
        {
            var post = new UpdatePost(postId, companyId, title, salary);
            var result = await _postRepository.UpdatePostAsync(post);
            await _cache.StringSetAsync($"post_{result.Id}", JsonSerializer.Serialize(result), TimeSpan.FromMinutes(5));
            _logger.LogInformation("Post with id {Id} was updated", postId);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while updating post with id {Id}", postId);
            throw;
        }
    }

    public async Task<PostPage> GetPostsByCompanyIdAsync(Guid companyId, int pageNumber, int pageSize)
    {
        try
        {
            if (!CacheDirty)
            {
                var postKeys = (await _cache.SetMembersAsync($"post_page_{companyId}_{pageNumber}_{pageSize}"))
                    .Select(k => (RedisKey)k.ToString())
                    .ToArray();

                var allValid = postKeys.Length == pageSize;

                foreach (var key in postKeys)
                {
                    if (!allValid)
                        break;
                    allValid = allValid && await _cache.KeyExistsAsync(key);
                }

                if (allValid)
                {
                    var items = new List<BasePost>();
                    foreach (var key in postKeys)
                        items.Add(JsonSerializer.Deserialize<BasePost>(await _cache.StringGetAsync(key)));
                    return new PostPage(
                        items.OrderBy(e => e.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                        new Page(pageNumber, (int)Math.Ceiling(items.Count / (double)pageSize), items.Count));
                }
                await DeleteCache();
            }
            else
            {
                await DeleteCache();
                CacheDirty = false;
            }

            var result = await _postRepository.GetPostsAsync(companyId, pageNumber, pageSize);

            foreach (var post in result.Posts)
            {
                await _cache.SetAddAsync($"post_page_{companyId}_{pageNumber}_{pageSize}",
                    $"post_{post.Id}");
                await _cache.StringSetAsync($"post_{post.Id}", JsonSerializer.Serialize(post),
                    TimeSpan.FromMinutes(10));
            }

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
            await _postRepository.DeletePostAsync(postId);
            await _cache.KeyDeleteAsync($"post_{postId}");
            PostHistoryService.PostHistoryService.CacheDirty = true;
            _logger.LogInformation("Post with id {Id} was deleted", postId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred while deleting post with id {Id}", postId);
            throw;
        }
    }

    private async Task DeleteCache()
    {
        await _cache.ExecuteAsync("FLUSHDB");
    }
}