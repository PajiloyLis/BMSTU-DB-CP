namespace Project.Core.Models.Education;

public class BaseEducation
{
    public BaseEducation(Guid id, Guid employeeId, string institution, string level, string studyField,
        DateOnly startDate, DateOnly? endDate)
    {
        if (!Guid.TryParse(id.ToString(), out _))
            throw new ArgumentException("Id is invalid", nameof(id));

        if (!Guid.TryParse(employeeId.ToString(), out _))
            throw new ArgumentException("EmployeeId is invalid", nameof(employeeId));

        if (string.IsNullOrWhiteSpace(institution))
            throw new ArgumentException("Institution is invalid", nameof(institution));

        if (string.IsNullOrWhiteSpace(studyField))
            throw new ArgumentException("StudyField is invalid", nameof(studyField));

        if (startDate > DateOnly.FromDateTime(DateTime.Today) || (endDate is not null && startDate > endDate))
            throw new ArgumentException("StartDate is invalid", nameof(startDate));

        if (endDate is not null && endDate > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("EndDate is invalid", nameof(endDate));

        Level = level.ToEducationLevel();
        Id = id;
        EmployeeId = employeeId;
        Institution = institution;
        StudyField = studyField;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string Institution { get; set; }
    public EducationLevel Level { get; set; }

    public string StudyField { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}