using Project.Core.Models;
using Project.Core.Models.Employee;

namespace Project.Core.Services;

public interface IEmployeeService
{
    Task<BaseEmployee> AddEmployeeAsync(string fullName, string phoneNumber, string email, DateOnly birthday,
        string? photoPath,
        string? duties);

    Task<BaseEmployee> UpdateEmployeeAsync(Guid id, string? fullName, string? phoneNumber, string? email,
        DateOnly? birthday,
        string? photoPath, string? duties);

    Task<BaseEmployee> GetEmployeeByIdAsync(Guid userId);

    Task<EmployeePage> GetSubordinatesByDirectorIdAsync(Guid directorId);

    Task DeleteEmployeeAsync(Guid userId);
}