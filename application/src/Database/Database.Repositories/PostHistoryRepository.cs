using Database.Context;
using Database.Models.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.PostHistory;
using Project.Core.Repositories;

namespace Database.Repositories;

public class PostHistoryRepository : IPostHistoryRepository
{
    private readonly ProjectDbContext _context;
    private readonly ILogger<PostHistoryRepository> _logger;

    public PostHistoryRepository(
        ProjectDbContext context,
        ILogger<PostHistoryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BasePostHistory> AddPostHistoryAsync(CreatePostHistory createPostHistory)
    {
        try
        {
            _logger.LogInformation("Adding post history for employee {EmployeeId} and post {PostId}",
                createPostHistory.EmployeeId, createPostHistory.PostId);

            var postHistoryDb = PostHistoryConverter.Convert(createPostHistory);
            await _context.PostHistoryDb.AddAsync(postHistoryDb);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully added post history for employee {EmployeeId} and post {PostId}",
                createPostHistory.EmployeeId, createPostHistory.PostId);

            return PostHistoryConverter.Convert(postHistoryDb)!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding post history for employee {EmployeeId} and post {PostId}",
                createPostHistory.EmployeeId, createPostHistory.PostId);
            throw;
        }
    }

    public async Task<BasePostHistory> GetPostHistoryByIdAsync(Guid postId, Guid employeeId)
    {
        try
        {
            _logger.LogInformation("Getting post history for employee {EmployeeId} and post {PostId}",
                employeeId, postId);

            var postHistoryDb = await _context.PostHistoryDb
                .FirstOrDefaultAsync(ph => ph.PostId == postId && ph.EmployeeId == employeeId);

            if (postHistoryDb == null)
            {
                _logger.LogWarning("Post history not found for employee {EmployeeId} and post {PostId}",
                    employeeId, postId);
                throw new PostHistoryNotFoundException(postId, employeeId);
            }

            _logger.LogInformation("Successfully retrieved post history for employee {EmployeeId} and post {PostId}",
                employeeId, postId);

            return PostHistoryConverter.Convert(postHistoryDb)!;
        }
        catch (PostHistoryNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post history for employee {EmployeeId} and post {PostId}",
                employeeId, postId);
            throw;
        }
    }

    public async Task<BasePostHistory> UpdatePostHistoryAsync(UpdatePostHistory updatePostHistory)
    {
        try
        {
            _logger.LogInformation("Updating post history for employee {EmployeeId} and post {PostId}",
                updatePostHistory.EmployeeId, updatePostHistory.PostId);

            var postHistoryDb = await _context.PostHistoryDb
                .FirstOrDefaultAsync(ph => ph.PostId == updatePostHistory.PostId &&
                                           ph.EmployeeId == updatePostHistory.EmployeeId);

            if (postHistoryDb == null)
            {
                _logger.LogWarning("Post history not found for employee {EmployeeId} and post {PostId}",
                    updatePostHistory.EmployeeId, updatePostHistory.PostId);
                throw new PostHistoryNotFoundException(updatePostHistory.PostId, updatePostHistory.EmployeeId);
            }

            if (updatePostHistory.StartDate.HasValue)
                postHistoryDb.StartDate = updatePostHistory.StartDate.Value;

            if (updatePostHistory.EndDate.HasValue)
                postHistoryDb.EndDate = updatePostHistory.EndDate.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated post history for employee {EmployeeId} and post {PostId}",
                updatePostHistory.EmployeeId, updatePostHistory.PostId);

            return PostHistoryConverter.Convert(postHistoryDb)!;
        }
        catch (PostHistoryNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post history for employee {EmployeeId} and post {PostId}",
                updatePostHistory.EmployeeId, updatePostHistory.PostId);
            throw;
        }
    }

    public async Task DeletePostHistoryAsync(Guid postId, Guid employeeId)
    {
        try
        {
            _logger.LogInformation("Deleting post history for employee {EmployeeId} and post {PostId}",
                employeeId, postId);

            var postHistoryDb = await _context.PostHistoryDb
                .FirstOrDefaultAsync(ph => ph.PostId == postId && ph.EmployeeId == employeeId);

            if (postHistoryDb == null)
            {
                _logger.LogWarning("Post history not found for employee {EmployeeId} and post {PostId}",
                    employeeId, postId);
                throw new PostHistoryNotFoundException(postId, employeeId);
            }

            _context.PostHistoryDb.Remove(postHistoryDb);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted post history for employee {EmployeeId} and post {PostId}",
                employeeId, postId);
        }
        catch (PostHistoryNotFoundException)
        {
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
            _logger.LogInformation(
                "Getting post history for employee {EmployeeId} from {StartDate} to {EndDate}, page {PageNumber}, size {PageSize}",
                employeeId, startDate, endDate, pageNumber, pageSize);

            var query = _context.PostHistoryDb
                .Where(ph => ph.EmployeeId == employeeId);
            
            if (startDate.HasValue)
                query = query.Where(ph => ph.EndDate == null || ph.EndDate >= startDate);
            if (endDate.HasValue)
                query = query.Where(ph =>
                    (ph.EndDate == null && endDate == DateOnly.FromDateTime(DateTime.Today)) || ph.EndDate <= endDate);

            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ph => PostHistoryConverter.Convert(ph)!)
                .ToListAsync();

            _logger.LogInformation(
                "Successfully retrieved {Count} post history records for employee {EmployeeId}",
                items.Count, employeeId);

            return new PostHistoryPage(items, new Page(pageNumber, (int)Math.Ceiling(totalItems/(double)pageSize), totalItems));
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
            _logger.LogInformation(
                "Getting subordinates post history for manager {ManagerId} from {StartDate} to {EndDate}, page {PageNumber}, size {PageSize}",
                managerId, startDate, endDate, pageNumber, pageSize);

            var employees = await _context.GetCurrentSubordinatesIdByEmployeeId(managerId).Select(ph => ph.EmployeeId)
                .ToListAsync();
            
            var query = _context.PostHistoryDb.Where(ph => employees.Contains(ph.EmployeeId));
            
            if (startDate.HasValue)
                query = query.Where(ph => ph.EndDate == null || ph.EndDate >= startDate);
            if (endDate.HasValue)
                query = query.Where(ph =>
                    (ph.EndDate == null && endDate == DateOnly.FromDateTime(DateTime.Today)) || ph.EndDate <= endDate);
            
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ph => PostHistoryConverter.Convert(ph)!)
                .ToListAsync();

            _logger.LogInformation(
                "Successfully retrieved {Count} subordinates post history records for manager {ManagerId}",
                items.Count, managerId);

            return new PostHistoryPage(items, new Page(pageNumber, (int)Math.Ceiling(totalItems/(double)pageSize), totalItems));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subordinates post history for manager {ManagerId}", managerId);
            throw;
        }
    }
}