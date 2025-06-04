using Project.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Services.AuthorizationService.Extensions;

public static class AuthorizationServiceProviderExtensions
{
    public static IServiceCollection AddAuthorizationService(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        return services;
    }
}