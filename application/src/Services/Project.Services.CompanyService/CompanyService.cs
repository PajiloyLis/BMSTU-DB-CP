using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Company;
using Project.Core.Repositories;
using Project.Core.Services;
using Project.Services.PostService;
using StackExchange.Redis;

namespace Project.Services.CompanyService;

public class CompanyService : ICompanyService
{
    private readonly IDatabaseAsync _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<CompanyService> _logger;
    private static bool _cacheDirty = false;

    public CompanyService(ICompanyRepository companyRepository, ILogger<CompanyService> logger, IConnectionMultiplexer cache)
    {
        _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionMultiplexer = cache ?? throw new ArgumentNullException(nameof(cache));
        _cache = cache.GetDatabase() ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<BaseCompany> AddCompanyAsync(string title, DateOnly registrationDate, string phoneNumber,
        string email, string inn, string kpp, string ogrn, string address)
    {
        try
        {
            var createdCompany = await _companyRepository.AddCompanyAsync(new CreationCompany(title, registrationDate,
                phoneNumber, email, inn, kpp, ogrn, address));
            await _cache.StringSetAsync($"company_{createdCompany.CompanyId}", JsonSerializer.Serialize(createdCompany), TimeSpan.FromMinutes(5));
            _cacheDirty = true;
            return createdCompany;
        }
        catch (CompanyAlreadyExistsException e)
        {
            _logger.LogWarning(e,
                $"Company with title - {title}, or phone - {phoneNumber}, or email - {email}, or inn - {inn}, or kpp - {kpp}, or ogrn - {ogrn} already exists");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error creating company - {title}");
            throw;
        }
    }

    public async Task<BaseCompany> GetCompanyByIdAsync(Guid companyId)
    {
        try
        {
            var cachedData = await _cache.StringGetAsync($"company_{companyId}");

            if (cachedData.HasValue)
                return JsonSerializer.Deserialize<BaseCompany>(cachedData);
            
            var company = await _companyRepository.GetCompanyByIdAsync(companyId);

            await _cache.StringSetAsync($"company_{companyId}", JsonSerializer.Serialize(company), TimeSpan.FromMinutes(5));
            return company;
        }
        catch (CompanyNotFoundException e)
        {
            _logger.LogWarning(e, $"Company with id - {companyId} not found");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error getting company with id - {companyId}");
            throw;
        }
    }

    public async Task<BaseCompany> UpdateCompanyAsync(Guid companyId, string? title, DateOnly? registrationDate,
        string? phoneNumber, string? email, string? inn,
        string? kpp, string? ogrn, string? address)
    {
        try
        {
            var company = await _companyRepository.UpdateCompanyAsync(new UpdateCompany(companyId, title,
                registrationDate, phoneNumber, email, inn, kpp, ogrn, address));

            await _cache.StringSetAsync($"company_{companyId}", JsonSerializer.Serialize(company), TimeSpan.FromMinutes(5));
            return company;
        }
        catch (CompanyAlreadyExistsException e)
        {
            _logger.LogWarning(e, "Company with such parameters already exists");
            throw;
        }
        catch (CompanyNotFoundException e)
        {
            _logger.LogWarning(e, $"Company with id - {companyId} not found");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error updating company with id {companyId}");
            throw;
        }
    }

    public async Task<CompanyPage> GetCompaniesAsync(int pageNumber, int pageSize)
    {
        try
        {
            // Если не было изменений испортивших закэшированные страницы 
            if (!_cacheDirty)
            {
                var companyKeys = (await _cache.SetMembersAsync($"company_page_{pageNumber}_{pageSize}"))
                    .Select(k => (RedisKey)k.ToString())
                    .ToArray();

                bool allValid = companyKeys.Length == pageSize;

                foreach (var key in companyKeys)
                {
                    if (!allValid)
                        break;
                    allValid = allValid && await _cache.KeyExistsAsync(key);
                }

                if (allValid)
                {
                    List<BaseCompany> items = new List<BaseCompany>();
                    foreach (var key in companyKeys)
                    {
                        items.Add(JsonSerializer.Deserialize<BaseCompany>(await _cache.StringGetAsync(key)));
                    }

                    return new CompanyPage(
                        items.OrderBy(e => e.RegistrationDate).Skip((pageNumber - 1) * pageSize).Take(pageSize)
                            .ToList(),
                        new Page(pageNumber, (int)Math.Ceiling(items.Count / (double)pageSize), items.Count));
                }
                await DeleteCache();
            }
            else
            {
                // Если были, сносим кэш
                await DeleteCache();
            }

            _cacheDirty = false;
            
            var companies = await _companyRepository.GetCompaniesAsync(pageNumber, pageSize);

            foreach (var item in companies.Companies)
            {
                await _cache.SetAddAsync($"company_page_{pageNumber}_{pageSize}", $"company_{item.CompanyId}");
                await _cache.StringSetAsync($"company_{item.CompanyId}", JsonSerializer.Serialize(item), TimeSpan.FromMinutes(10));
            }
            
            return companies;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting companies");
            throw;
        }
    }

    public async Task DeleteCompanyAsync(Guid companyId)
    {
        try
        {
            await _companyRepository.DeleteCompanyAsync(companyId);
            await _cache.KeyDeleteAsync($"company_{companyId}");
            _cacheDirty = true;
            PostService.PostService.CacheDirty = true;
            PositionService.PositionService.CacheDirty = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error deleting company with id - {companyId}");
            throw;
        }
    }
    
    private async Task DeleteCache()
    {
        await _cache.ExecuteAsync("FLUSHDB");
    }
}