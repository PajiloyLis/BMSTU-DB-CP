namespace Project.Dto.Http.Position;

public class PositionHierarchyDto
{
    public PositionHierarchyDto(Guid positionId, Guid? parentId, string title, int level)
    {
        PositionId = positionId;
        ParentId = parentId;
        Title = title;
        Level = level;
    }
    public Guid PositionId { get; set; }
    public Guid? ParentId { get; set; }
    public string Title { get; set; }
    public int Level { get; set; }
}