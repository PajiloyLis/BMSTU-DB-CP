using Project.Core.Models;
using Project.Core.Models.Company;

namespace Project.Core.Repositories;

/// <summary>
/// Company repository
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Asynchronous company addition method
    /// </summary>
    /// <param name="newCompany"><see cref="CreationCompany"/>> model to add</param>
    /// <exception cref="Project.Core.Exceptions.CompanyAlreadyExistsException">
    /// If company with one of unique parameters
    /// already exists
    /// </exception>
    /// <returns><see cref="BaseCompany"/> model representing added company entity</returns>
    public Task<BaseCompany> AddCompanyAsync(CreationCompany newCompany);

    /// <summary>
    /// Asynchronous search company by
    /// </summary>
    /// <param name="companyId"></param>
    /// <returns></returns>
    public Task<BaseCompany> GetCompanyByIdAsync(Guid companyId);

    public Task<BaseCompany> UpdateCompanyAsync(UpdateCompany company);

    public Task<CompanyPage> GetCompaniesAsync(int pageNumber, int pageSize);

    public Task DeleteCompanyAsync(Guid companyId);
}