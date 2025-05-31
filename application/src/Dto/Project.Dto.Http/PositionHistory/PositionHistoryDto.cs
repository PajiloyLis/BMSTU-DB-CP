namespace Project.Dto.Http.PositionHistory;

public class PositionHistoryDto
{
    public PositionHistoryDto(
        Guid positionId,
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate)
    {
        PositionId = positionId;
        EmployeeId = employeeId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid PositionId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}