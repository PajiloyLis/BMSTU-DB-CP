using Project.Core.Models;
using Project.Core.Models.Employee;

namespace Project.Core.Repositories;

public interface IEmployeeRepository
{
    Task<BaseEmployee> AddEmployeeAsync(CreationEmployee newEmployee);

    Task<BaseEmployee> UpdateEmployeeAsync(UpdateEmployee updateEmployee);

    Task<BaseEmployee> GetEmployeeByIdAsync(Guid employeeId);

    Task<EmployeePage> GetSubordinatesByDirectorIdAsync(Guid directorId);

    Task DeleteEmployeeAsync(Guid employeeId);
}