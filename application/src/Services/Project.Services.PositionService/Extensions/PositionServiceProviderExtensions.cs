using Microsoft.Extensions.DependencyInjection;
using Project.Core.Services;

namespace Project.Services.PositionService.Extensions;

public static class PositionServiceProviderExtensions
{
    public static IServiceCollection AddPositionService(this IServiceCollection services)
    {
        services.AddScoped<IPositionService, PositionService>();
        return services;
    }
}