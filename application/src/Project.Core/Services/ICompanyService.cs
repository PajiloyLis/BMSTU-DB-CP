using Project.Core.Models;
using Project.Core.Models.Company;

namespace Project.Core.Services;

public interface ICompanyService
{
    Task<BaseCompany> AddCompanyAsync(string title, DateOnly registrationDate, string phoneNumber,
        string email, string inn, string kpp, string ogrn, string address);

    Task<BaseCompany> GetCompanyByIdAsync(Guid companyId);

    Task<BaseCompany> UpdateCompanyAsync(Guid companyId, string? title, DateOnly? registrationDate, string? phoneNumber,
        string? email, string? inn, string? kpp, string? ogrn, string? address);

    Task<CompanyPage> GetCompaniesAsync(int pageNumber, int pageSize);

    Task DeleteCompanyAsync(Guid companyId);
}