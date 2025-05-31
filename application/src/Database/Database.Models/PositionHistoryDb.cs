using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Database.Models;

namespace Project.Database.Models;

public class PositionHistoryDb
{
    public PositionHistoryDb(Guid positionId, Guid employeeId, DateOnly startDate, DateOnly? endDate = null)
    {
        PositionId = positionId;
        EmployeeId = employeeId;
        StartDate = startDate;
        EndDate = endDate;
    }

    [ForeignKey(nameof(PositionDb))]
    public Guid PositionId { get; set; }
    [ForeignKey(nameof(EmployeeDb))]
    public Guid EmployeeId { get; set; }
    [Required]
    public DateOnly StartDate { get; set; }
    [Required]
    public DateOnly? EndDate { get; set; }
}