using System.Diagnostics.CodeAnalysis;
using Project.Core.Models;
using Project.Core.Models.Employee;

namespace Database.Models.Converters;

public static class EmployeeConverter
{
    [return: NotNullIfNotNull(nameof(employee))]
    public static BaseEmployee? Convert(EmployeeDb? employee)
    {
        if (employee is null) return null;

        return new BaseEmployee(employee.Id,
            employee.FullName,
            employee.Phone,
            employee.Email,
            employee.BirthDate,
            employee.Photo,
            employee.Duties
        );
    }

    // [return: NotNullIfNotNull(nameof(employee))]
    // public static EmployeeDb? Convert(UpdateEmployee? employee)
    // {
    //     if (employee == null)
    //         return null;
    //
    //     return new EmployeeDb(employee.EmployeeId,
    //         employee.FullName,
    //         employee.PhoneNumber,
    //         employee.Email,
    //         employee.BirthDate,
    //         employee.Photo,
    //         employee.Duties
    //     );
    // }

    [return: NotNullIfNotNull(nameof(employee))]
    public static EmployeeDb? Convert(CreationEmployee? employee)
    {
        if (employee == null)
            return null;

        return new EmployeeDb(Guid.NewGuid(),
            employee.FullName,
            employee.PhoneNumber,
            employee.Email,
            employee.BirthDate,
            employee.Photo,
            employee.Duties
        );
    }

    [return: NotNullIfNotNull(nameof(employee))]
    public static EmployeeDb? Convert(BaseEmployee? employee)
    {
        if (employee == null)
            return null;

        return new EmployeeDb(Guid.NewGuid(),
            employee.FullName,
            employee.PhoneNumber,
            employee.Email,
            employee.BirthDate,
            employee.Photo,
            employee.Duties
        );
    }
}