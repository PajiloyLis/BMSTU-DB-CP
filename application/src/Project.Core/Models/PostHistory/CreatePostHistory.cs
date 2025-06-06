namespace Project.Core.Models.PostHistory;

public class CreatePostHistory
{
    public CreatePostHistory(
        Guid postId,
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate)
    {
        if (postId == Guid.Empty)
            throw new ArgumentException("Post ID cannot be empty", nameof(postId));

        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty", nameof(employeeId));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (startDate >= today)
            throw new ArgumentException("Start date cannot be in the future", nameof(startDate));

        if (endDate is not null)
        {
            if (endDate.Value > today)
                throw new ArgumentException("End date cannot be in the future", nameof(endDate));

            if (endDate.Value <= startDate)
                throw new ArgumentException("End date must be after start date", nameof(endDate));
        }

        PostId = postId;
        EmployeeId = employeeId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid PostId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}