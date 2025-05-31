using Microsoft.Extensions.DependencyInjection;
using Project.Core.Services;

namespace Project.Services.CompanyService.Extensions;

public static class CompanyServiceProviderExtensions
{
    public static IServiceCollection AddCompanyService(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();
        return services;
    }
}