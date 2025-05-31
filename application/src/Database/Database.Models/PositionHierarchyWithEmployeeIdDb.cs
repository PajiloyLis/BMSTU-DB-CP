using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

public class PositionHierarchyWithEmployeeIdDb
{
        public PositionHierarchyWithEmployeeIdDb(Guid employeeId, Guid positionId, Guid? parentId, string title, int level)
        {
                EmployeeId = employeeId;
                PositionId = positionId;
                ParentId = parentId;
                Title = title;
                Level = level;
        }
        [Column("employee_id")]
        public Guid EmployeeId { get; set; }
        [Column("position_id")]
        public Guid PositionId { get; set; }
        [Column("parent_id")]
        public Guid? ParentId { get; set; }
        [Column("title")]
        public string Title { get; set; }
        [Column("level")]
        public int Level { get; set; }
}