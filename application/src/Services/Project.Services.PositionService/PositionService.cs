using System.Text.Json;
using Microsoft.Extensions.Logging;
using Project.Core.Models;
using Project.Core.Models.Position;
using Project.Core.Models.PositionHistory;
using Project.Core.Repositories;
using Project.Core.Services;
using StackExchange.Redis;

namespace Project.Services.PositionService;

public class PositionService : IPositionService
{
    private readonly ILogger<PositionService> _logger;
    private readonly IPositionRepository _repository;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabaseAsync _cache;
    public static bool CacheDirty = false;

    public PositionService(IPositionRepository repository, ILogger<PositionService> logger, IConnectionMultiplexer connectionMultiplexer)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionMultiplexer =
            connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _cache = _connectionMultiplexer.GetDatabase();
    }

    public async Task<BasePosition> AddPositionAsync(Guid? parentId, string title, Guid companyId)
    {
        try
        {
            var model = new CreatePosition(parentId, title, companyId);
            var result = await _repository.AddPositionAsync(model);
            await _cache.StringSetAsync($"position_{result.Id}", JsonSerializer.Serialize(result),
                TimeSpan.FromMinutes(5));
            _logger.LogInformation("Position added: {Id}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding position");
            throw;
        }
    }

    public async Task<BasePosition> GetPositionByIdAsync(Guid id)
    {
        try
        {
            if (!CacheDirty)
            {
                var cachedData = await _cache.StringGetAsync($"position_{id}");
                if (cachedData.HasValue)
                    return JsonSerializer.Deserialize<BasePosition>(cachedData);
            }
            else
            {
                await DeleteCache();
                CacheDirty = false;
            }
            var result = await _repository.GetPositionByIdAsync(id);
            await _cache.StringSetAsync($"position_{result.Id}", JsonSerializer.Serialize(result),
                TimeSpan.FromMinutes(5));
            _logger.LogInformation("Position retrieved: {Id}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting position by id {Id}", id);
            throw;
        }
    }

    public async Task<BasePosition> GetHeadPositionByCompanyIdAsync(Guid id)
    {
        try
        {
            var result = await _repository.GetHeadPositionByCompanyIdAsync(id);
            return result;
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Error getting head position for company with id {id}");
            throw;
        }
    }

    public async Task<BasePosition> UpdatePositionTitleAsync(Guid id, Guid companyId, Guid? parentId = null,
        string? title = null)
    {
        try
        {
            var model = new UpdatePosition(id, companyId, parentId, title);
            var result = await _repository.UpdatePositionTitleAsync(model);
            await _cache.StringSetAsync($"position_{result.Id}", JsonSerializer.Serialize(result),
                TimeSpan.FromMinutes(5));
            _logger.LogInformation("Position updated: {Id}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating position {Id}", id);
            throw;
        }
    }

    public async Task<BasePosition> UpdatePositionParentWithSubordinatesAsync(Guid id, Guid companyId, Guid? parentId = null, string? title = null)
    {
        try
        {
            if (parentId is null)
            {
                _logger.LogWarning("Parent id must be not null");
                throw new ArgumentNullException(nameof(parentId), "Parent id must be not null");
            }
            var model = new UpdatePosition(id, companyId, parentId, title);
            var result = await _repository.UpdatePositionParentWithSubordinatesAsync(model);
            await _cache.StringSetAsync($"position_{result.Id}", JsonSerializer.Serialize(result),
                TimeSpan.FromMinutes(5));
            _logger.LogInformation("Position updated: {Id}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating position {Id}", id);
            throw;
        }
    }

    public async Task<BasePosition> UpdatePositionParentWithoutSuboridnatesAsync(Guid id, Guid companyId, Guid? parentId = null, string? title = null)
    {
        try
        {
            if (parentId is null)
            {
                _logger.LogWarning("Parent id must be not null");
                throw new ArgumentNullException(nameof(parentId), "Parent id must be not null");
            }
            var model = new UpdatePosition(id, companyId, parentId, title);
            var result = await _repository.UpdatePositionParentWithoutSuboridnatesAsync(model);
            await _cache.StringSetAsync($"position_{result.Id}", JsonSerializer.Serialize(result),
                TimeSpan.FromMinutes(5));
            _logger.LogInformation("Position updated: {Id}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating position {Id}", id);
            throw;
        }
    }

    public async Task DeletePositionAsync(Guid id)
    {
        try
        {
            await _repository.DeletePositionAsync(id);
            await _cache.KeyDeleteAsync($"position_{id}");
            CacheDirty = true;
            PositionHistoryService.PositionHistoryService.CacheDirty = false;
            _logger.LogInformation("Position deleted: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting position {Id}", id);
            throw;
        }
    }

    public async Task<PositionHierarchyPage> GetSubordinatesAsync(Guid parentId, int pageNumber, int pageSize)
    {
        try
        {
            var result = await _repository.GetSubordinatesAsync(parentId, pageNumber, pageSize);
            _logger.LogInformation("Subordinates retrieved for parentId: {ParentId}", parentId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subordinates for parentId {ParentId}", parentId);
            throw;
        }
    }
    
    private async Task DeleteCache()
    {
        await _cache.ExecuteAsync("FLUSHDB");
    }
}