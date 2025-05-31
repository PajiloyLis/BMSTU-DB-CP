using System.Text.Json;
using Database.Context;
using Database.Models.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Education;
using Project.Core.Repositories;

namespace Database.Repositories;

public class EducationRepository : IEducationRepository
{
    private readonly ProjectDbContext _context;
    private readonly ILogger<EducationRepository> _logger;

    public EducationRepository(ProjectDbContext context, ILogger<EducationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BaseEducation> AddEducationAsync(CreateEducation education)
    {
        try
        {
            var educationDb = EducationConverter.Convert(education);
            
            var existingEducation = await _context.EducationDb
                .FirstOrDefaultAsync(e => e.EmployeeId == educationDb.EmployeeId &&
                                          e.Institution == educationDb.Institution &&
                                          e.StudyField == educationDb.StudyField &&
                                          e.StartDate == educationDb.StartDate &&
                                          e.EndDate == educationDb.EndDate);

            if (existingEducation is not null)
            {
                _logger.LogWarning("Education already exists for employee {EmployeeId}", education.EmployeeId);
                throw new EducationAlreadyExistsException(
                    $"Education already exists for employee {education.EmployeeId}");
            }
            
            await _context.EducationDb.AddAsync(educationDb);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Education with id {Id} was added for employee {EmployeeId}",
                educationDb.Id, education.EmployeeId);

            return EducationConverter.Convert(educationDb);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding education for employee {EmployeeId}", education.EmployeeId);
            throw;
        }
    }

    public async Task<BaseEducation> GetEducationByIdAsync(Guid educationId)
    {
        try
        {
            var educationDb = await _context.EducationDb
                .FirstOrDefaultAsync(e => e.Id == educationId);
            
            if (educationDb is null)
            {
                _logger.LogWarning("Education with id {Id} not found", educationId);
                throw new EducationNotFoundException($"Education with id {educationId} not found");
            }

            _logger.LogInformation("Education with id {Id} was found", educationId);
            return EducationConverter.Convert(educationDb);
        }
        catch (Exception e) when (e is not EducationNotFoundException)
        {
            _logger.LogError(e, "Error getting education with id {Id}", educationId);
            throw;
        }
    }

    public async Task<BaseEducation> UpdateEducationAsync(UpdateEducation education)
    {
        try
        {
            var educationDb = await _context.EducationDb
                .FirstOrDefaultAsync(e => e.Id == education.Id && e.EmployeeId == education.EmployeeId);

            if (educationDb is null)
            {
                _logger.LogWarning("Education with id {Id} not found for update", education.Id);
                throw new EducationNotFoundException($"Education with id {education.Id} not found");
            }

            var existingEducation = await _context.EducationDb
                .Where(e => e.Id != education.Id &&
                            e.EmployeeId == education.EmployeeId &&
                            e.Institution == education.Institution &&
                            e.StudyField == education.StudyField &&
                            e.StartDate == education.StartDate &&
                            e.EndDate == education.EndDate)
                .FirstOrDefaultAsync();

            if (existingEducation is not null)
            {
                _logger.LogWarning("Education record already exists for employee {EmployeeId}", education.EmployeeId);
                throw new EducationAlreadyExistsException("Education record already exists");
            }

            educationDb.Institution = education.Institution ?? educationDb.Institution;
            educationDb.StudyField = education.StudyField ?? educationDb.StudyField;
            if (education.Level is not null)
                educationDb.Level = education.Level.ToStringVal();
            educationDb.StartDate = education.StartDate ?? educationDb.StartDate;
            educationDb.EndDate = education.EndDate ?? educationDb.EndDate;
            
            await _context.SaveChangesAsync();

            _logger.LogInformation("Education with id {Id} was updated", education.Id);
            return EducationConverter.Convert(educationDb);
        }
        catch (Exception e) when (e is not EducationNotFoundException)
        {
            _logger.LogError(e, "Error updating education with id {Id}", education.Id);
            throw;
        }
    }

    public async Task<EducationPage> GetEducationsAsync(Guid employeeId, int pageNumber, int pageSize)
    {
        try
        {
            var query = _context.EducationDb.Where(e => e.EmployeeId == employeeId).OrderBy(db => db.StartDate);
            var totalCount = await query.CountAsync();

            var educations = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(e => EducationConverter.Convert(e))
                .ToListAsync();

            _logger.LogInformation("Got {Count} educations for employee {EmployeeId} (page {Page}, size {Size})",
                educations.Count, employeeId, pageNumber, pageSize);

            return new EducationPage(educations, new Page(pageNumber, (int)Math.Ceiling(totalCount/(double)pageSize), totalCount));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting educations for employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task DeleteEducationAsync(Guid educationId)
    {
        try
        {
            
            var educationDb = await _context.EducationDb
                .FirstOrDefaultAsync(e => e.Id == educationId);

            if (educationDb is null)
            {
                _logger.LogWarning("Education with id {Id} not found for deletion", educationId);
                throw new EducationNotFoundException($"Education with id {educationId} not found");
            }

            _context.EducationDb.Remove(educationDb);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Education with id {Id} was deleted", educationId);
        }
        catch (Exception e) when (e is not EducationNotFoundException)
        {
            _logger.LogError(e, "Error deleting education with id {Id}", educationId);
            throw;
        }
    }
}