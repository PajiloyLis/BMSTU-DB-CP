using Microsoft.Extensions.DependencyInjection;
using Project.Core.Services;

namespace Project.Services.PostHistoryService.Extensions;

public static class PostHistoryServiceProviderExtensions
{
    public static IServiceCollection AddPostHistoryService(this IServiceCollection services)
    {
        services.AddScoped<IPostHistoryService, PostHistoryService>();
        return services;
    }
}