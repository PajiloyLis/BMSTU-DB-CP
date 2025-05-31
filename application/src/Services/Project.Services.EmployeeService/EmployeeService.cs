using System.Text.Json;
using Microsoft.Extensions.Logging;
using Project.Core.Exceptions;
using Project.Core.Models;
using Project.Core.Models.Employee;
using Project.Core.Repositories;
using Project.Core.Services;
using StackExchange.Redis;

namespace Project.Services.EmployeeService;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IDatabaseAsync _cache;
    private static bool _cacheDirty = false;

    public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger, IConnectionMultiplexer cache)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionMultiplexer = cache ?? throw new ArgumentNullException(nameof(cache));
        _cache = cache.GetDatabase() ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<BaseEmployee> AddEmployeeAsync(string fullName, string phoneNumber, string email, DateOnly birthday,
        string? photoPath, string? duties)
    {
        try
        {
            var employeeToAdd = new CreationEmployee(fullName, phoneNumber, email, birthday, photoPath, duties);
            var employee = await _employeeRepository.AddEmployeeAsync(employeeToAdd);

            await _cache.StringSetAsync($"employee_{employee.EmployeeId}", JsonSerializer.Serialize(employee), TimeSpan.FromMinutes(5));
            _cacheDirty = true;
            return employee;
        }
        catch (EmployeeAlreadyExistsException e)
        {
            _logger.LogWarning(e,
                $"Employee with email - {email} or phone number - {phoneNumber} or generated id already exists");
            throw;
        }
        catch (ArgumentException e)
        {
            _logger.LogWarning(e, "Employee with incorrect parameters passed");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error creating employee with email - {email} and phone number - {phoneNumber}");
            throw;
        }
    }

    public async Task<BaseEmployee> UpdateEmployeeAsync(Guid id, string? fullName, string? phoneNumber, string? email,
        DateOnly? birthday,
        string? photoPath, string? duties)
    {
        try
        {
            var employeeToUpdate = new UpdateEmployee(id, fullName, phoneNumber, email, birthday, photoPath, duties);
            var employee = await _employeeRepository.UpdateEmployeeAsync(employeeToUpdate);
            await _cache.StringSetAsync($"employee_{id}", JsonSerializer.Serialize(employee), TimeSpan.FromMinutes(5));
            return employee;
        }
        catch (EmployeeAlreadyExistsException e)
        {
            _logger.LogWarning(e, "Employee with such parameters already exists");
            throw;
        }
        catch (EmployeeNotFoundException e)
        {
            _logger.LogWarning(e, $"Employee with id - {id} not found");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error updating employee with id - {id}");
            throw;
        }
    }

    public async Task DeleteEmployeeAsync(Guid employeeId)
    {
        try
        {
            await _employeeRepository.DeleteEmployeeAsync(employeeId);
            await _cache.KeyDeleteAsync($"employee_{employeeId}");
            _cacheDirty = true;
            EducationService.EducationService.CacheDirty = true;
        }
        catch (EmployeeNotFoundException e)
        {
            _logger.LogWarning(e, $"Employee with id - {employeeId} not found");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error deleting employee with id - {employeeId}");
            throw;
        }
    }

    public async Task<BaseEmployee> GetEmployeeByIdAsync(Guid id)
    {
        try
        {
            var cachedData = await _cache.StringGetAsync($"employee_{id}");
            if (cachedData.HasValue)
            {
                return JsonSerializer.Deserialize<BaseEmployee>(cachedData);
            }
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            await _cache.StringSetAsync($"employee_{id}", JsonSerializer.Serialize(employee), TimeSpan.FromMinutes(5));
            return employee;
        }
        catch (EmployeeNotFoundException e)
        {
            _logger.LogWarning(e, $"Employee with id - {id} not found");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error getting employee with id - {id}");
            throw;
        }
    }

    public async Task<EmployeePage> GetSubordinatesByDirectorIdAsync(Guid directorId)
    {
        try
        {
            var employees = await _employeeRepository.GetSubordinatesByDirectorIdAsync(directorId);
            return employees;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error getting employees by director id - {directorId}");
            throw;
        }
    }
}