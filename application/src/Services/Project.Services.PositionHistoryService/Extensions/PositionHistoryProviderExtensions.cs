using Microsoft.Extensions.DependencyInjection;
using Project.Core.Services;

namespace Project.Services.PositionHistoryService.Extensions;

public static class PositionHistoryProviderExtensions
{
    public static IServiceCollection AddPositionHistoryService(this IServiceCollection services)
    {
        services.AddScoped<IPositionHistoryService, PositionHistoryService>();
        return services;
    }
}