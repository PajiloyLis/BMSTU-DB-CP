using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

public class PositionHierarchyDb
{
    public PositionHierarchyDb(Guid positionId, Guid? parentId, string title, int level)
    {
        PositionId = positionId;
        ParentId = parentId;
        Title = title;
        Level = level;
    }
    
    [Column("id")]
    public Guid PositionId { get; set; }
    [Column("parent_id")]
    public Guid? ParentId { get; set; }
    [Column("title")]
    public string Title { get; set; }
    [Column("level")]
    public int Level { get; set; }
}