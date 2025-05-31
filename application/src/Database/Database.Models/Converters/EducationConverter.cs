using System.Diagnostics.CodeAnalysis;
using Project.Core.Models;
using Project.Core.Models.Education;

namespace Database.Models.Converters;

public static class EducationConverter
{
    [return: NotNullIfNotNull(nameof(education))]
    public static EducationDb? Convert(CreateEducation? education)
    {
        if (education == null)
            return null;

        return new EducationDb(Guid.NewGuid(),
            education.EmployeeId,
            education.Institution,
            (education.Level as EducationLevel?).ToStringVal(),
            education.StudyField,
            education.StartDate,
            education.EndDate
        );
    }

    [return: NotNullIfNotNull(nameof(education))]
    public static EducationDb? Convert(BaseEducation? education)
    {
        if (education == null)
            return null;

        return new EducationDb(education.Id,
            education.EmployeeId,
            education.Institution,
            (education.Level as EducationLevel?).ToStringVal(),
            education.StudyField,
            education.StartDate,
            education.EndDate);
    }

    [return: NotNullIfNotNull(nameof(education))]
    public static BaseEducation? Convert(EducationDb? education)
    {
        if (education == null)
            return null;

        return new BaseEducation(education.Id,
            education.EmployeeId,
            education.Institution,
            education.Level,
            education.StudyField,
            education.StartDate,
            education.EndDate);
    }
}