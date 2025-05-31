using Project.Core.Models;
using Project.Core.Models.Education;

namespace Project.Core.Services;

public interface IEducationService
{
    Task<BaseEducation> AddEducationAsync(Guid employeeId, string institution, string level, string studyField,
        DateOnly startDate, DateOnly? endDate = null);

    Task<BaseEducation> GetEducationByIdAsync(Guid educationId);

    Task<BaseEducation> UpdateEducationAsync(Guid educationId, Guid employeeId, string? institution = null,
        string? level = null, string? studyField = null, DateOnly? startDate = null, DateOnly? endDate = null);

    Task<EducationPage> GetEducationsByEmployeeIdAsync(Guid employeeId, int pageNumber, int pageSize);

    Task DeleteEducationAsync(Guid educationId);
}