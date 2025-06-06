using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

public class PostHistoryDb
{
    public PostHistoryDb()
    {
    }

    public PostHistoryDb(
        Guid postId,
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate = null)
    {
        PostId = postId;
        EmployeeId = employeeId;
        StartDate = startDate;
        EndDate = endDate;
    }
    [Column("post_id")]
    [ForeignKey(nameof(PostDb))]
    public Guid PostId { get; set; }
    
    [Column("employee_id")]
    [ForeignKey(nameof(EmployeeDb))]
    public Guid EmployeeId { get; set; }
    [Required]
    public DateOnly StartDate { get; set; }
    [Required]
    public DateOnly? EndDate { get; set; }
}