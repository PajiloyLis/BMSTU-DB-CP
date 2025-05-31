using Microsoft.Extensions.DependencyInjection;
using Project.Core.Services;

namespace Project.Services.EducationService.Extensions;

public static class EducationServiceProviderExtensions
{
    public static IServiceCollection AddEducationService(this IServiceCollection services)
    {
        services.AddScoped<IEducationService, EducationService>();
        return services;
    }
}