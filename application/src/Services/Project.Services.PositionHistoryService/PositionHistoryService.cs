using System.Text.Json;
using Microsoft.Extensions.Logging;
using Project.Core.Exceptions;
using Project.Core.Models.PositionHistory;
using Project.Core.Repositories;
using Project.Core.Services;
using StackExchange.Redis;

namespace Project.Services.PositionHistoryService;

public class PositionHistoryService : IPositionHistoryService
{
    public static bool CacheDirty;
    private readonly IDatabaseAsync _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<PositionHistoryService> _logger;
    private readonly IPositionHistoryRepository _repository;

    public PositionHistoryService(
        IPositionHistoryRepository repository,
        ILogger<PositionHistoryService> logger, IConnectionMultiplexer cache)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionMultiplexer = cache ?? throw new ArgumentNullException(nameof(cache));
        _cache = cache.GetDatabase() ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<BasePositionHistory> AddPositionHistoryAsync(
        Guid positionId,
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate = null)
    {
        try
        {
            _logger.LogInformation(
                "Adding position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);

            var createPositionHistory = new CreatePositionHistory(
                positionId,
                employeeId,
                startDate,
                endDate);

            var result = await _repository.AddPositionHistoryAsync(createPositionHistory);
            await _cache.StringSetAsync($"position_history_{positionId}_{employeeId}", JsonSerializer.Serialize(result),
                TimeSpan.FromMinutes(5));
            _logger.LogInformation(
                "Successfully added position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);
            throw;
        }
    }

    public async Task<BasePositionHistory> GetPositionHistoryAsync(Guid positionId, Guid employeeId)
    {
        try
        {
            if (!CacheDirty)
            {
                var cachedData = await _cache.StringGetAsync($"position_history_{positionId}_{employeeId}");
                if (cachedData.HasValue)
                    return JsonSerializer.Deserialize<BasePositionHistory>(cachedData);
            }
            else
            {
                await DeleteCache();
                CacheDirty = false;
            }

            _logger.LogInformation(
                "Getting position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);

            var result = await _repository.GetPositionHistoryByIdAsync(positionId, employeeId);
            
            await _cache.StringSetAsync($"position_history_{positionId}_{employeeId}", JsonSerializer.Serialize(result), TimeSpan.FromMinutes(5));

            _logger.LogInformation(
                "Successfully retrieved position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);

            return result;
        }
        catch (PositionHistoryNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);
            throw;
        }
    }

    public async Task<BasePositionHistory> UpdatePositionHistoryAsync(
        Guid positionId,
        Guid employeeId,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        try
        {
            _logger.LogInformation(
                "Updating position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);

            var updatePositionHistory = new UpdatePositionHistory(
                positionId,
                employeeId,
                startDate,
                endDate);

            var result = await _repository.UpdatePositionHistoryAsync(updatePositionHistory);
            
            await _cache.StringSetAsync($"position_history_{positionId}_{employeeId}", JsonSerializer.Serialize(result), TimeSpan.FromMinutes(5));

            _logger.LogInformation(
                "Successfully updated position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);

            return result;
        }
        catch (PositionHistoryNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);
            throw;
        }
    }

    public async Task DeletePositionHistoryAsync(Guid positionId, Guid employeeId)
    {
        try
        {
            _logger.LogInformation(
                "Deleting position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);

            await _repository.DeletePositionHistoryAsync(positionId, employeeId);
            await _cache.KeyDeleteAsync($"position_history_{positionId}_{employeeId}");
            CacheDirty = true;
            _logger.LogInformation(
                "Successfully deleted position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);
        }
        catch (PositionHistoryNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);
            throw;
        }
    }

    public async Task<PositionHistoryPage> GetPositionHistoryByEmployeeIdAsync(Guid employeeId,
        int pageNumber,
        int pageSize,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        try
        {
            _logger.LogInformation(
                "Getting position history for employee {EmployeeId} from {StartDate} to {EndDate}, page {PageNumber}, size {PageSize}",
                employeeId, startDate, endDate, pageNumber, pageSize);

            var result = await _repository.GetPositionHistoryByEmployeeIdAsync(
                employeeId,
                pageNumber,
                pageSize,
                startDate,
                endDate);

            _logger.LogInformation(
                "Successfully retrieved {Count} position history records for employee {EmployeeId}",
                result.Items.Count, employeeId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting position history for employee {EmployeeId}",
                employeeId);
            throw;
        }
    }

    public async Task<PositionHierarchyWithEmployeePage> GetCurrentSubordinatesAsync(
        Guid managerId,
        int pageNumber,
        int pageSize)
    {
        try
        {
            _logger.LogInformation(
                "Getting current subordinates position history for manager {ManagerId}, page {PageNumber}, size {PageSize}",
                managerId, pageNumber, pageSize);

            var result = await _repository.GetCurrentSubordinatesAsync(
                managerId,
                pageNumber,
                pageSize);

            _logger.LogInformation(
                "Successfully retrieved {Count} current subordinates position history records for manager {ManagerId}",
                result.Items.Count, managerId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting current subordinates position history for manager {ManagerId}",
                managerId);
            throw;
        }
    }

    public async Task<PositionHistoryPage> GetCurrentSubordinatesPositionHistoryAsync(
        Guid managerId,
        int pageNumber,
        int pageSize,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        try
        {
            _logger.LogInformation(
                "Getting current subordinates position history for manager {ManagerId}, page {PageNumber}, size {PageSize}",
                managerId, pageNumber, pageSize);

            var result = await _repository.GetCurrentSubordinatesPositionHistoryAsync(
                managerId,
                pageNumber,
                pageSize,
                startDate,
                endDate);

            _logger.LogInformation(
                "Successfully retrieved {Count} current subordinates position history records for manager {ManagerId}",
                result.Items.Count, managerId);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting current subordinates position history for manager {ManagerId}",
                managerId);
            throw;
        }
    }

    private async Task DeleteCache()
    {
        await _cache.ExecuteAsync("FLUSHDB");
    }
}