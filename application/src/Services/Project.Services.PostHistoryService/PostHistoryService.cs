using System.Text.Json;
using Microsoft.Extensions.Logging;
using Project.Core.Exceptions;
using Project.Core.Models.PostHistory;
using Project.Core.Repositories;
using Project.Core.Services;
using StackExchange.Redis;

namespace Project.Services.PostHistoryService;

public class PostHistoryService : IPostHistoryService
{
    public static bool CacheDirty;
    private readonly IDatabaseAsync _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<PostHistoryService> _logger;
    private readonly IPostHistoryRepository _repository;

    public PostHistoryService(
        IPostHistoryRepository repository,
        ILogger<PostHistoryService> logger, IConnectionMultiplexer cache)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionMultiplexer = cache ?? throw new ArgumentNullException(nameof(cache));
        _cache = cache.GetDatabase() ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<BasePostHistory> AddPostHistoryAsync(
        Guid postId,
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate = null)
    {
        try
        {
            var createPostHistory = new CreatePostHistory(postId, employeeId, startDate, endDate);
            var postHistory = await _repository.AddPostHistoryAsync(createPostHistory);
            await _cache.StringSetAsync($"post_history_{postId}_{employeeId}", JsonSerializer.Serialize(postHistory),
                TimeSpan.FromMinutes(5));
            CacheDirty = true;
            return postHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding post history for employee {EmployeeId} and post {PostId}",
                employeeId, postId);
            throw;
        }
    }

    public async Task<BasePostHistory> GetPostHistoryAsync(Guid postId, Guid employeeId)
    {
        try
        {
            if (!CacheDirty)
            {
                var cachedData = await _cache.StringGetAsync($"post_history_{postId}_{employeeId}");
                if (cachedData.HasValue)
                    return JsonSerializer.Deserialize<BasePostHistory>(cachedData);
            }
            else
            {
                await DeleteCache();
                CacheDirty = false;
            }

            var postHistory = await _repository.GetPostHistoryByIdAsync(postId, employeeId);
            await _cache.StringSetAsync($"post_history_{postId}_{employeeId}", JsonSerializer.Serialize(postHistory),
                TimeSpan.FromMinutes(5));
            return postHistory;
        }
        catch (PostHistoryNotFoundException)
        {
            _logger.LogWarning("Post history not found for employee {EmployeeId} and post {PostId}",
                employeeId, postId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post history for employee {EmployeeId} and post {PostId}",
                employeeId, postId);
            throw;
        }
    }

    public async Task<BasePostHistory> UpdatePostHistoryAsync(
        Guid postId,
        Guid employeeId,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        try
        {
            var updatePostHistory = new UpdatePostHistory(postId, employeeId, startDate, endDate);
            var postHistory = await _repository.UpdatePostHistoryAsync(updatePostHistory);
            await _cache.StringSetAsync($"post_history_{postId}_{employeeId}", JsonSerializer.Serialize(postHistory),
                TimeSpan.FromMinutes(5));
            return postHistory;
        }
        catch (PostHistoryNotFoundException)
        {
            _logger.LogWarning("Post history not found for employee {EmployeeId} and post {PostId}",
                employeeId, postId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post history for employee {EmployeeId} and post {PostId}",
                employeeId, postId);
            throw;
        }
    }

    public async Task DeletePostHistoryAsync(Guid postId, Guid employeeId)
    {
        try
        {
            await _repository.DeletePostHistoryAsync(postId, employeeId);
            await _cache.KeyDeleteAsync($"post_history_{postId}_{employeeId}");
            CacheDirty = true;
        }
        catch (PostHistoryNotFoundException)
        {
            _logger.LogWarning("Post history not found for employee {EmployeeId} and post {PostId}",
                employeeId, postId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post history for employee {EmployeeId} and post {PostId}",
                employeeId, postId);
            throw;
        }
    }

    public async Task<PostHistoryPage> GetPostHistoryByEmployeeIdAsync(
        Guid employeeId,
        int pageNumber,
        int pageSize,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        try
        {
            return await _repository.GetPostHistoryByEmployeeIdAsync(
                employeeId,
                pageNumber,
                pageSize,
                startDate,
                endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post history for employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<PostHistoryPage> GetSubordinatesPostHistoryAsync(
        Guid managerId,
        int pageNumber,
        int pageSize,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        try
        {
            return await _repository.GetSubordinatesPostHistoryAsync(
                managerId,
                pageNumber,
                pageSize,
                startDate,
                endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subordinates post history for manager {ManagerId}", managerId);
            throw;
        }
    }

    private async Task DeleteCache()
    {
        await _cache.ExecuteAsync("FLUSHDB");
    }
}