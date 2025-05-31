namespace Project.Dto.Http.PositionHistory;

public class PositionHierarchyWithEmployeeDto
{
    public PositionHierarchyWithEmployeeDto(
        Guid employeeId,
        Guid positionId,
        Guid? parentId,
        string title,
        int level)
    {
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