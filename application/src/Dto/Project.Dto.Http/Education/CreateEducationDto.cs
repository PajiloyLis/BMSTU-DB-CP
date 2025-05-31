namespace Project.Dto.Http.Education;

public class CreateEducationDto
{
    public CreateEducationDto(Guid employeeId, string institution, string level, string studyField,
        DateOnly startDate, DateOnly? endDate)
    {
        Level = level;
        EmployeeId = employeeId;
        Institution = institution;
        StudyField = studyField;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid EmployeeId { get; set; }
    public string Institution { get; set; }
    public string Level { get; set; }

    public string StudyField { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}