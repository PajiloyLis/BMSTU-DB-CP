using Database.Context;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.PositionHistory;
using Project.Core.Repositories;
using Project.Database.Models.Converters;

namespace Project.Database.Repositories;

public class PositionHistoryRepository : IPositionHistoryRepository
{
    private readonly ProjectDbContext _context;
    private readonly ILogger<PositionHistoryRepository> _logger;

    public PositionHistoryRepository(
        ProjectDbContext context,
        ILogger<PositionHistoryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BasePositionHistory> AddPositionHistoryAsync(CreatePositionHistory createPositionHistory)
    {
        try
        {
            _logger.LogInformation(
                "Adding position history for employee {EmployeeId} and position {PositionId}",
                createPositionHistory.EmployeeId, createPositionHistory.PositionId);

            var positionHistoryDb = PositionHistoryConverter.Convert(createPositionHistory);
            if (positionHistoryDb == null)
                throw new ArgumentNullException(nameof(createPositionHistory));
            await _context.PositionHistoryDb.AddAsync(positionHistoryDb);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully added position history for employee {EmployeeId} and position {PositionId}",
                createPositionHistory.EmployeeId, createPositionHistory.PositionId);

            return PositionHistoryConverter.Convert(positionHistoryDb)!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding position history for employee {EmployeeId} and position {PositionId}",
                createPositionHistory.EmployeeId, createPositionHistory.PositionId);
            throw;
        }
    }

    public async Task<BasePositionHistory> GetPositionHistoryByIdAsync(Guid positionId, Guid employeeId)
    {
        try
        {
            _logger.LogInformation(
                "Getting position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);

            var positionHistoryDb = await _context.PositionHistoryDb
                .FirstOrDefaultAsync(x => x.PositionId == positionId && x.EmployeeId == employeeId);

            if (positionHistoryDb == null)
            {
                _logger.LogWarning(
                    "Position history not found for employee {EmployeeId} and position {PositionId}",
                    employeeId, positionId);
                throw new PositionHistoryNotFoundException();
            }

            _logger.LogInformation(
                "Successfully retrieved position history for employee {EmployeeId} and position {PositionId}",
                employeeId, positionId);

            return PositionHistoryConverter.Convert(positionHistoryDb)!;
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

    public async Task<BasePositionHistory> UpdatePositionHistoryAsync(UpdatePositionHistory updatePositionHistory)
    {
        try
        {
            _logger.LogInformation(
                "Updating position history for employee {EmployeeId} and position {PositionId}",
                updatePositionHistory.EmployeeId, updatePositionHistory.PositionId);

            var positionHistoryDb = await _context.PositionHistoryDb
                .FirstOrDefaultAsync(x => x.PositionId == updatePositionHistory.PositionId &&
                                          x.EmployeeId == updatePositionHistory.EmployeeId);

            if (positionHistoryDb == null)
            {
                _logger.LogWarning(
                    "Position history not found for employee {EmployeeId} and position {PositionId}",
                    updatePositionHistory.EmployeeId, updatePositionHistory.PositionId);
                throw new PositionHistoryNotFoundException();
            }

            if (updatePositionHistory.StartDate.HasValue)
                positionHistoryDb.StartDate = updatePositionHistory.StartDate.Value;

            if (updatePositionHistory.EndDate.HasValue)
                positionHistoryDb.EndDate = updatePositionHistory.EndDate.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully updated position history for employee {EmployeeId} and position {PositionId}",
                updatePositionHistory.EmployeeId, updatePositionHistory.PositionId);

            return PositionHistoryConverter.Convert(positionHistoryDb)!;
        }
        catch (PositionHistoryNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating position history for employee {EmployeeId} and position {PositionId}",
                updatePositionHistory.EmployeeId, updatePositionHistory.PositionId);
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

            var positionHistoryDb = await _context.PositionHistoryDb
                .FirstOrDefaultAsync(x => x.PositionId == positionId && x.EmployeeId == employeeId);

            if (positionHistoryDb == null)
            {
                _logger.LogWarning(
                    "Position history not found for employee {EmployeeId} and position {PositionId}",
                    employeeId, positionId);
                throw new PositionHistoryNotFoundException();
            }

            _context.PositionHistoryDb.Remove(positionHistoryDb);
            await _context.SaveChangesAsync();

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

    public async Task<PositionHistoryPage> GetPositionHistoryByEmployeeIdAsync(
        Guid employeeId,
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

            var query = _context.PositionHistoryDb
                .Where(x => x.EmployeeId == employeeId);

            if (startDate.HasValue)
                query = query.Where(s => s.EndDate == null || s.EndDate >= startDate);
            if (endDate.HasValue)
                query = query.Where(s =>
                    (s.EndDate == null && endDate == DateOnly.FromDateTime(DateTime.Today)) || s.EndDate <= endDate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderByDescending(x => x.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => PositionHistoryConverter.Convert(x)!)
                .ToListAsync();

            _logger.LogInformation(
                "Successfully retrieved {Count} position history records for employee {EmployeeId}",
                items.Count, employeeId);

            return new PositionHistoryPage(items, new Page(pageNumber, totalPages, totalCount));
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

            var query = _context.GetCurrentSubordinatesIdByEmployeeId(managerId)
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Title);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => PositionHierarchyWithEmployeeIdConverter.Convert(x)!)
                .ToListAsync();

            _logger.LogInformation(
                "Successfully retrieved {Count} current subordinates position history records for manager {ManagerId}",
                items.Count, managerId);

            return new PositionHierarchyWithEmployeePage(items, new Page(pageNumber, totalPages, totalCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting current subordinates position history for manager {ManagerId}",
                managerId);
            throw;
        }
    }

    public async Task<BasePositionHistory> GetCurrentEmployeePositionByEmployeeIdAsync(Guid employeeId)
    {
        try
        {
            _logger.LogInformation($"Getting current position for employee {employeeId}");

            var cur_pos =
                await _context.PositionHistoryDb.FirstOrDefaultAsync(ph =>
                    ph.EmployeeId == employeeId && ph.EndDate == null);
            if (cur_pos is null)
                throw new PositionHistoryNotFoundException($"Position history not found for employee {employeeId}");

            return PositionHistoryConverter.Convert(cur_pos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting current position for employee {employeeId}");
            throw;
        }
    }

    public async Task<PositionHistoryPage> GetCurrentSubordinatesPositionHistoryAsync(
        Guid managerId,
        int pageNumber,
        int pageSize, DateOnly? startDate, DateOnly? endDate)
    {
        try
        {
            _logger.LogInformation(
                "Getting current subordinates position history for manager {ManagerId}, page {PageNumber}, size {PageSize}",
                managerId, pageNumber, pageSize);

            var employees = await _context.GetCurrentSubordinatesIdByEmployeeId(managerId).Select(ph => ph.EmployeeId)
                .ToListAsync();

            var query = _context.PositionHistoryDb.Where(ph => employees.Contains(ph.EmployeeId));

            if (startDate.HasValue)
                query = query.Where(ph => ph.EndDate == null || ph.EndDate >= startDate);
            if (endDate.HasValue)
                query = query.Where(ph =>
                    (ph.EndDate == null && endDate == DateOnly.FromDateTime(DateTime.Today)) || ph.EndDate <= endDate);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => PositionHistoryConverter.Convert(x)!)
                .ToListAsync();

            _logger.LogInformation(
                "Successfully retrieved {Count} current subordinates position history records for manager {ManagerId}",
                items.Count, managerId);

            return new PositionHistoryPage(items, new Page(pageNumber, totalPages, totalCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting current subordinates position history for manager {ManagerId}",
                managerId);
            throw;
        }
    }
}