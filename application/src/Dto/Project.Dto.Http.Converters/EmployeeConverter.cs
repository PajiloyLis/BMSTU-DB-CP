using System.Diagnostics.CodeAnalysis;
using Project.Core.Models;
using Project.Core.Models.Employee;
using Project.Dto.Http.Employee;

namespace Project.Dto.Http.Converters;

public static class EmployeeConverter
{
    [return: NotNullIfNotNull(nameof(employee))]
    public static EmployeeDto? Convert(BaseEmployee? employee)
    {
        if (employee is null)
            return null;

        return new EmployeeDto(employee.EmployeeId,
            employee.FullName,
            employee.PhoneNumber,
            employee.Email,
            employee.BirthDate,
            employee.Photo,
            employee.Duties
        );
    }
}