namespace Project.Core.Models.PositionHistory;

public class UpdatePositionHistory
{
    public UpdatePositionHistory(
        Guid positionId,
        Guid employeeId,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        if (positionId == Guid.Empty)
            throw new ArgumentException("Position ID cannot be empty", nameof(positionId));

        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty", nameof(employeeId));

        if (startDate is not null && startDate.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Start date must be in the past", nameof(startDate));

        if (endDate is not null)
        {
            if (endDate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("End date cannot be in the future", nameof(endDate));

            if (startDate.HasValue && endDate.Value <= startDate.Value)
                throw new ArgumentException("End date must be after start date", nameof(endDate));
        }

        PositionId = positionId;
        EmployeeId = employeeId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid PositionId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}