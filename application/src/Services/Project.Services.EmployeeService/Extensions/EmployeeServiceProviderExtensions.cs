using Microsoft.Extensions.DependencyInjection;
using Project.Core.Services;

namespace Project.Services.EmployeeService.Extensions;

public static class EmployeeServiceProviderExtensions
{
    public static IServiceCollection AddEmployeeService(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        return services;
    }
}