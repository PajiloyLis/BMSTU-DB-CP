using System.Text.Json;
using Microsoft.Extensions.Logging;
using Project.Core.Models;
using Project.Core.Models.Education;
using Project.Core.Repositories;
using Project.Core.Services;
using StackExchange.Redis;

namespace Project.Services.EducationService;

public class EducationService : IEducationService
{
    public static bool CacheDirty;
    private readonly IDatabaseAsync _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IEducationRepository _educationRepository;
    private readonly ILogger<EducationService> _logger;

    public EducationService(IEducationRepository educationRepository, ILogger<EducationService> logger,
        IConnectionMultiplexer cache)
    {
        _educationRepository = educationRepository ?? throw new ArgumentNullException(nameof(educationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionMultiplexer = cache ?? throw new ArgumentNullException(nameof(cache));
        _cache = cache.GetDatabase() ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<BaseEducation> AddEducationAsync(Guid employeeId, string institution, string level,
        string studyField,
        DateOnly startDate, DateOnly? endDate = null)
    {
        try
        {
            var createdEducation = await _educationRepository.AddEducationAsync(
                new CreateEducation(employeeId, institution, level, studyField, startDate, endDate));

            await _cache.StringSetAsync($"education_{createdEducation.Id}", JsonSerializer.Serialize(createdEducation),
                TimeSpan.FromMinutes(5));
            CacheDirty = true;
            return createdEducation;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error creating education for employee {employeeId}");
            throw;
        }
    }

    public async Task<BaseEducation> GetEducationByIdAsync(Guid educationId)
    {
        try
        {
            if (!CacheDirty)
            {
                var cachedValue = await _cache.StringGetAsync($"education_{educationId}");
                if (cachedValue.HasValue)
                    return JsonSerializer.Deserialize<BaseEducation>(cachedValue);
            }
            else
            {
                await DeleteCache();
                CacheDirty = false;
            }
            var education = await _educationRepository.GetEducationByIdAsync(educationId);

            await _cache.StringSetAsync($"education_{educationId}", JsonSerializer.Serialize(education),
                TimeSpan.FromMinutes(5));
            return education;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error getting education with id {educationId}");
            throw;
        }
    }

    public async Task<BaseEducation> UpdateEducationAsync(Guid educationId, Guid employeeId, string? institution = null,
        string? level = null, string? studyField = null, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        try
        {
            var education = await _educationRepository.UpdateEducationAsync(
                new UpdateEducation(educationId, employeeId, institution, level, studyField, startDate, endDate));

            await _cache.StringSetAsync($"education_{educationId}", JsonSerializer.Serialize(education),
                TimeSpan.FromMinutes(5));

            return education;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error updating education with id {educationId}");
            throw;
        }
    }

    public async Task<EducationPage> GetEducationsByEmployeeIdAsync(Guid employeeId, int pageNumber, int pageSize)
    {
        try
        {
            if (!CacheDirty)
            {
                var educationKeys = (await _cache.SetMembersAsync($"education_page_{employeeId}_{pageNumber}_{pageSize}"))
                    .Select(k => (RedisKey)k.ToString())
                    .ToArray();

                    var allValid = educationKeys.Length == pageSize;

                foreach (var key in educationKeys)
                {
                    if (!allValid)
                        break;
                    allValid = allValid && await _cache.KeyExistsAsync(key);
                }

                if (allValid)
                {
                    var items = new List<BaseEducation>();
                    foreach (var key in educationKeys)
                        items.Add(JsonSerializer.Deserialize<BaseEducation>(await _cache.StringGetAsync(key)));
                    return new EducationPage(
                        items.OrderBy(e => e.StartDate).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                        new Page(pageNumber, (int)Math.Ceiling(items.Count / (double)pageSize), items.Count));
                }
                await DeleteCache();
            }
            else
            {
                await DeleteCache();
                CacheDirty = false;
            }

            await _cache.KeyDeleteAsync($"education_page_{employeeId}_");
            var educations = await _educationRepository.GetEducationsAsync(employeeId, pageNumber, pageSize);

            foreach (var education in educations.Educations)
            {
                await _cache.SetAddAsync($"education_page_{employeeId}_{pageNumber}_{pageSize}",
                    $"education_{education.Id}");
                await _cache.StringSetAsync($"education_{education.Id}", JsonSerializer.Serialize(education),
                    TimeSpan.FromMinutes(10));
            }

            return educations;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error getting educations for employee {employeeId}");
            throw;
        }
    }

    public async Task DeleteEducationAsync(Guid educationId)
    {
        try
        {
            await _educationRepository.DeleteEducationAsync(educationId);
            await _cache.KeyDeleteAsync($"education_{educationId}");
            CacheDirty = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error deleting education with id {educationId}");
            throw;
        }
    }

    private async Task DeleteCache()
    {
        await _cache.ExecuteAsync("FLUSHDB");
    }
}