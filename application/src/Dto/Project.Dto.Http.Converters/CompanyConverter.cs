using System.Diagnostics.CodeAnalysis;
using Project.Core.Models.Company;
using Project.Dto.Http.Company;

namespace Project.Dto.Http.Converters;

public static class CompanyConverter
{
    [return: NotNullIfNotNull(nameof(company))]
    public static CompanyDto? Convert(BaseCompany? company)
    {
        if (company is null)
            return null;

        return new CompanyDto(company.CompanyId,
            company.Title,
            company.RegistrationDate,
            company.PhoneNumber,
            company.Email,
            company.Inn,
            company.Kpp,
            company.Ogrn,
            company.Address
        );
    }
}