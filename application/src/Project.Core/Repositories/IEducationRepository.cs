using Project.Core.Models;
using Project.Core.Models.Education;

namespace Project.Core.Repositories;

public interface IEducationRepository
{
    Task<BaseEducation> AddEducationAsync(CreateEducation education);
    Task<BaseEducation> GetEducationByIdAsync(Guid educationId);
    Task<BaseEducation> UpdateEducationAsync(UpdateEducation education);
    Task<EducationPage> GetEducationsAsync(Guid employeeId, int pageNumber, int pageSize);
    Task DeleteEducationAsync(Guid educationId);
}