namespace Project.Core.Models.Education;

public class UpdateEducation
{
    public UpdateEducation(Guid id, Guid employeeId, string? institution = null, string? level = null,
        string? studyField = null,
        DateOnly? startDate = null, DateOnly? endDate = null)
    {
        if (!Guid.TryParse(id.ToString(), out _))
            throw new ArgumentException("Id is invalid", nameof(id));

        if (!Guid.TryParse(employeeId.ToString(), out _))
            throw new ArgumentException("EmployeeId is invalid", nameof(employeeId));

        if (startDate is not null && endDate is not null && startDate > endDate)
            throw new ArgumentException("StartDate is invalid", nameof(startDate));

        if (startDate is not null && startDate > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("StartDate is invalid", nameof(startDate));

        if (endDate is not null && endDate > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("EndDate is invalid", nameof(endDate));

        if (level is not null)
            Level = level.ToEducationLevel();
        else
            Level = null;

        Id = id;
        EmployeeId = employeeId;
        Institution = institution;
        StudyField = studyField;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? Institution { get; set; }
    public EducationLevel? Level { get; set; }
    public string? StudyField { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}