using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.PositionHistory;
using Project.Database.Models;

namespace Database.Models;

public static class PositionHierarchyWithEmployeeIdConverter
{
    [return: NotNullIfNotNull("positionHierarchy")]
    public static PositionHierarchyWithEmployeeIdDb? Convert(PositionHierarchyWithEmployee? positionHierarchy)
    {
        if (positionHierarchy == null)
            return null;

        return new PositionHierarchyWithEmployeeIdDb(
            positionHierarchy.EmployeeId,
            positionHierarchy.PositionId,
            positionHierarchy.ParentId,
            positionHierarchy.Title,
            positionHierarchy.Level);
    }

    [return: NotNullIfNotNull("positionHierarchyDb")]
    public static PositionHierarchyWithEmployee? Convert(PositionHierarchyWithEmployeeIdDb? positionHierarchyDb)
    {
        if (positionHierarchyDb == null)
            return null;

        return new PositionHierarchyWithEmployee(
            positionHierarchyDb.EmployeeId,
            positionHierarchyDb.PositionId,
            positionHierarchyDb.ParentId,
            positionHierarchyDb.Title,
            positionHierarchyDb.Level);
    }
}