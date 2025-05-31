using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.Education;
using Project.Dto.Http.Education;

namespace Project.Dto.Http.Converters;

public static class EducationConverter
{
    [return: NotNullIfNotNull(nameof(education))]
    public static EducationDto? Convert(BaseEducation? education)
    {
        if (education is null)
            return null;

        return new EducationDto(education.Id,
            education.EmployeeId,
            education.Institution,
            (education.Level as EducationLevel?).ToStringVal(),
            education.StudyField,
            education.StartDate,
            education.EndDate
        );
    }
}