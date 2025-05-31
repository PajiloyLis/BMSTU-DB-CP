namespace Project.Dto.Http.PostHistory;

public class UpdatePostHistoryDto
{
    public UpdatePostHistoryDto(
        Guid postId,
        Guid employeeId,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        PostId = postId;
        EmployeeId = employeeId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid PostId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}