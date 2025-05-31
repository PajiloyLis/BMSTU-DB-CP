namespace Project.Dto.Http.PositionHistory;

public class CreatePositionHistoryDto
{
    public CreatePositionHistoryDto(
        Guid positionId,
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate = null)
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