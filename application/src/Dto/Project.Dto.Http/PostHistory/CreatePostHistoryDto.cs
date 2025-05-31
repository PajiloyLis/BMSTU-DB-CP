namespace Project.Dto.Http.PostHistory;

public class CreatePostHistoryDto
{
    public CreatePostHistoryDto(
        Guid postId,
        Guid employeeId,
        DateOnly startDate,
        DateOnly? endDate)
    {
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