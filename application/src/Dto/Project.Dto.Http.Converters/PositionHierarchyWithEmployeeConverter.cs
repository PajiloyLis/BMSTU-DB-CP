using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.PositionHistory;
using Project.Dto.Http.PositionHistory;

namespace Project.Dto.Http.Converters;

public static class PositionHierarchyWithEmployeeWithEmployeeConverter
{
    [return: NotNullIfNotNull(nameof(positionHierarchyWithEmployee))]
    public static PositionHierarchyWithEmployeeDto? Convert(PositionHierarchyWithEmployee? positionHierarchyWithEmployee)
    {
        if (positionHierarchyWithEmployee is null)
            return null;

        return new PositionHierarchyWithEmployeeDto(positionHierarchyWithEmployee.EmployeeId,
            positionHierarchyWithEmployee.PositionId,
            positionHierarchyWithEmployee.ParentId,
            positionHierarchyWithEmployee.Title,
            positionHierarchyWithEmployee.Level
        );
    }
}