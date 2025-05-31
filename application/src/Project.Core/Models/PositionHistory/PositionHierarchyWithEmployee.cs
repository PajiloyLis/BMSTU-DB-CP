namespace Project.Core.Models.PositionHistory;

public class PositionHierarchyWithEmployee
{
    public PositionHierarchyWithEmployee(
        Guid employeeId,
        Guid positionId,
        Guid? parentId,
        string title,
        int level)
    {
        if (positionId == Guid.Empty)
            throw new ArgumentException("Position ID cannot be empty", nameof(positionId));

        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty", nameof(employeeId));

        if (string.IsNullOrEmpty(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));
        if (level < 0)
            throw new ArgumentException("Level cannot be negative", nameof(level));

        PositionId = positionId;
        EmployeeId = employeeId;
        ParentId = parentId;
        Title = title;
        Level = level;
    }

    public Guid PositionId { get; set; }
    public Guid EmployeeId { get; set; }
    
    public Guid? ParentId { get; set; }
    public string Title { get; set; }
    public int Level { get; set; }
}