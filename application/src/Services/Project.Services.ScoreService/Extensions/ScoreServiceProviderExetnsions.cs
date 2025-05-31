using Microsoft.Extensions.DependencyInjection;
using Project.Core.Services;

namespace Project.Services.ScoreService.Extensions;

public static class ScoreServiceProviderExtensions
{
    public static IServiceCollection AddScoreService(this IServiceCollection services)
    {
        services.AddScoped<IScoreService, ScoreService>();
        return services;
    }
}